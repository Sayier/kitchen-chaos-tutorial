using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinLobbyStarted;
    public event EventHandler OnJoinLobbyFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby joinedLobby;

    private float heartbeatTimer;
    private const float HeartbeatTimerMax = 15f;
    public float listLobbiesTimer;
    private const float ListLobbiesTimerMax = 3f;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("LobbyManager instance has already been created.");
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleListLobbiesUpdate();
    }

    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State == ServicesInitializationState.Initialized)
        {
            return;
        }

        InitializationOptions initializationOptions = new();
        initializationOptions.SetProfile(UnityEngine.Random.Range(0, 1000).ToString());

        await UnityServices.InitializeAsync(initializationOptions);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MultiplayerManager.MaxPlayerAmount, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            MultiplayerManager.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        } 
        catch(LobbyServiceException e)
        {
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void QuickJoin()
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
    }
    
    public async void JoinLobbyById(string lobbyId)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
    }

    private async void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = HeartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private void HandleListLobbiesUpdate()
    {
        //Can not view lobbies if not logged into Unity Auth Service, player may not be logged in on first frame
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        //No need to update the lobby list if the player is already in a lobby
        if(joinedLobby != null)
        {
            return;
        }

        listLobbiesTimer -= Time.deltaTime;

        if(listLobbiesTimer <= 0f)
        {
            listLobbiesTimer = ListLobbiesTimerMax;

            ListLobbies();
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void CloseLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
