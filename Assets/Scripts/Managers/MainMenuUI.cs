using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button playSinglePlayerButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playMultiplayerButton.onClick.AddListener(() =>
        {
            MultiplayerManager.isPlayMultiplayer = true;
            Loader.Load(Loader.Scene.LobbyScene);
        });

        playSinglePlayerButton.onClick.AddListener(() =>
        {
            MultiplayerManager.isPlayMultiplayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });

        quitButton.onClick.AddListener(() => 
        {
            Application.Quit();
        });

        //Resets time scale in case main menu is loaded from the game scene's pause screen
        Time.timeScale = 1f;
    }

    private void Start()
    {
        playMultiplayerButton.Select();
    }
}
