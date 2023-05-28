using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI messageText;

    [SerializeField] private LobbyUI lobbyUI;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        MultiplayerManager.Instance.OnFailedToJoinGame += MultiplayerManager_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFailed += LobbyManager_OnJoinLobbyFailed;
        lobbyUI.OnNoLobbyCodeInput += LobbyUI_OnNoLobbyCodeInput;

        Hide();
    }

    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnFailedToJoinGame -= MultiplayerManager_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted -= LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed -= LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinLobbyStarted -= LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFailed -= LobbyManager_OnJoinLobbyFailed;
    }

    private void LobbyUI_OnNoLobbyCodeInput(object sender, System.EventArgs e)
    {
        ShowMessage("Please input a lobby code to join.");
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join lobby");
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining lobby...");
    }

    private void MultiplayerManager_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            messageText.text = "Failed to connect";
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating lobby...");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create lobby.");
    }

    private void ShowMessage(string newMessageText)
    {
        messageText.text = newMessageText;

        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
