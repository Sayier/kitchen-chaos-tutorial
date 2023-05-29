using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerLoadingUI : MonoBehaviour
{
    private void Start()
    {
        if(MultiplayerManager.isPlayMultiplayer == true)
        {
            Hide();
        }
        else
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
