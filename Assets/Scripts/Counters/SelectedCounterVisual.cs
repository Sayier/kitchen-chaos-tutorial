using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualSelectedGameObjectArray;

    private void Start()
    {
        Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
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
