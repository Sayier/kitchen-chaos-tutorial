using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] private GameObject visualStoveOnGameObject;
    [SerializeField] private GameObject particlesStoveOnGameObject;

    private StoveCounter stoveCounter;

    private void Awake()
    {
        stoveCounter = GetComponentInParent<StoveCounter>();
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool showVisual = e.cookingState == StoveCounter.State.Frying || e.cookingState == StoveCounter.State.Fried;

        visualStoveOnGameObject.SetActive(showVisual);
        particlesStoveOnGameObject.SetActive(showVisual);

    }
}
