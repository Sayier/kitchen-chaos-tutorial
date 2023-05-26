using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReady += GameManager_OnLocalPlayerReady;
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

        Hide();
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void GameManager_OnLocalPlayerReady(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetIsLocalPlayerReady() && GameManager.Instance.IsWaitingToStartActive())
        {
            Show();
        }
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
