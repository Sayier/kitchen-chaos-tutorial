using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;  
    [SerializeField] private Button createLobbyButton;  
    [SerializeField] private Button quickJoinLobbyButton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        createLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby("LobbyName", false);
        });

        quickJoinLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.QuickJoin();
        });
    }
}
