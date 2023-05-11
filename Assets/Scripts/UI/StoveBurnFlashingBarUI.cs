using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IsFlashing = "IsFlashing";

    Animator animator;
    StoveCounter stoveCounter;
    private readonly float warningSoundTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stoveCounter = GetComponentInParent<StoveCounter>();
    }

    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        //Only make the progress bar flash if the object is already fried and the burn timer is nearing completion
        float burnShowProgressAmount = .5f;
        bool showFlashing = stoveCounter.IsFried() && (e.progressNormalized >= burnShowProgressAmount);

        animator.SetBool(IsFlashing, showFlashing);
    }
}
