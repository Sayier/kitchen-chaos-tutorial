using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Lobbies.Models;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;  
    [SerializeField] private Button createLobbyButton;  
    [SerializeField] private Button quickJoinLobbyButton;
    [SerializeField] private Button joinCodeButton;

    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;

    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    public event EventHandler OnNoLobbyCodeInput;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        createLobbyButton.onClick.AddListener(() => {
            lobbyCreateUI.Show();
        });

        quickJoinLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.QuickJoin();
        });

        joinCodeButton.onClick.AddListener(() => {
            if (lobbyCodeInputField.text != "")
            {
                LobbyManager.Instance.JoinLobbyByCode(lobbyCodeInputField.text);
            }
            else
            {
                OnNoLobbyCodeInput?.Invoke(this, EventArgs.Empty);
            }
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = MultiplayerManager.Instance.GetPlayerName();

        playerNameInputField.onValueChanged.AddListener((string newText) => 
        {
            MultiplayerManager.Instance.SetPlayerName(newText);
        });

        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs eventArgs)
    {
        UpdateLobbyList(eventArgs.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate)
            {
                continue;
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        foreach(Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }
}
