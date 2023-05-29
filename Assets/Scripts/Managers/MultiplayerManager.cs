using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    public static bool isPlayMultiplayer;

    public const int MaxPlayerAmount = 4;
    private const int PlayerDataIndexNotFound = -1;
    private const string PlayerPrefsPlayerNameMultiplayer = "PlayerNameMultiplayer";

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

        playerName = PlayerPrefs.GetString(PlayerPrefsPlayerNameMultiplayer, $"PlayerName{UnityEngine.Random.Range(100,999)}");
    }

    private void Start()
    {
        if (!isPlayMultiplayer)
        {
            StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId(),
            playerName = GetPlayerName(),
            playerId = AuthenticationService.Instance.PlayerId
        });
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if(playerDataNetworkList[i].clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        //Serialize the KitchenObjectSO to an index in the KitchenObjectListSO and the IKitchenObjectParent
        //to a NetworkObjectReferense to be able to make the ServerRPC call to spawn the object
        SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        //Retrieve the KitchenObjectSO from the kitchenObjectSOIndex and Instantiate the prefab
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        
        //Get which KitchenObjectParent should be associated with the spawned KitchenObject by pulling it from
        //the from the NetworkObject in the passed NetworkObjectReference
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        if (kitchenObjectParent.HasKitchenObject())
        {
            //Parent has already spawned an object
            return;
        }

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        //Set the newly spawned KitchenObject to this parent
        KitchenObject spawnedKitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        spawnedKitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    //Serialize the kitchenObject as a NetworkObject and request the server to destroy that object
    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {    
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        //Retrieve the KitchenObject from the NetworkObjectReference
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);

        if (kitchenObjectNetworkObject == null)
        {
            //Make sure the kitchenObjectNetworkObject actually exists before continuing
            //Can already be destroyed depending on network lad and extra inputs
            return;
        }
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        //Inform all of the clients that the KitchenObject's parent needs to be unset in preperation for the Destory call
        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);

        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    public void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        //Retrieve the KitchenObject from the NetworkObjectReference and then clear the KitchenObjectParent
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        kitchenObject.ClearKitchenObjectOnParent();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //Only allow players to connect when still in the CharacterSelectScene
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game currently in progress";
            return;
        }
        //Do not allow player to connect if already at max lobby capacity
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MaxPlayerAmount)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game lobby already full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public bool IsPlayerIndexConnected(int playerIndex) 
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public void SetPlayerColor(int colorId)
    {
        SetPlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvaliable(colorId))
        {
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        if(playerDataIndex == PlayerDataIndexNotFound)
        {
            return;
        }

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.colorId = colorId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvaliable(int colorId)
    {
        bool isColorAvaliable = true;
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.colorId == colorId)
            {
                isColorAvaliable = false;
                break;
            }
        }

        return isColorAvaliable;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if(playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetFirstUnusedColorId()
    {
        for(int i = 0; i< playerColorList.Count; i++)
        {
            if (IsColorAvaliable(i))
            {
                return i;
            }
        }

        Debug.LogError("All player colors in use");
        return -1;
    }

    public void KickPlayer(int playerIndex)
    {
        PlayerData playerData = GetPlayerDataFromPlayerIndex(playerIndex);
        NetworkManager.Singleton.DisconnectClient(playerData.clientId);

        NetworkManager_Server_OnClientDisconnectCallback(playerData.clientId);
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        PlayerPrefs.SetString(PlayerPrefsPlayerNameMultiplayer, playerName);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string newPlayerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        if (playerDataIndex == PlayerDataIndexNotFound)
        {
            return;
        }

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = newPlayerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string newPlayerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        if (playerDataIndex == PlayerDataIndexNotFound)
        {
            return;
        }

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = newPlayerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
}
