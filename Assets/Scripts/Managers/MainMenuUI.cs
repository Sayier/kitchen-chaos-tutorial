using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
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
        playButton.Select();
    }
}
