using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualSelectedGameObjectArray;

    private void Start()
    {
        //If the local player instance exists subscribe the the selected counter event
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        //If the local player does not exist subscribe to the event to be informed a new player spawned
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        //If the local player exists unsubscribe from the selected counter event if it already exists
        //and then subscribe to the selected counter event
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == baseCounter)
        {
            ShowSelected();
        }
        else
        {
            HideSelected();
        }
    }

    private void ShowSelected()
    {
        foreach(GameObject visual in visualSelectedGameObjectArray) 
        {
            visual.SetActive(true);
        }   
    }

    private void HideSelected()
    {
        foreach (GameObject visual in visualSelectedGameObjectArray)
        {
            visual.SetActive(false);
        }
    }
}
