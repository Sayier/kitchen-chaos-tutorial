using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    private AudioSource audioSource;
    private StoveCounter stoveCounter;
    private float warningSoundTimer;
    private bool playWarningSound;
    private float volume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        stoveCounter = GetComponentInParent<StoveCounter>();
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;

        SoundManager.Instance.OnSoundEffectsVolumeChanged += SoundManager_OnSoundEffectsVolumeChanged;
        GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
        GameManager.Instance.OnLocalGameUnpaused += GameManager_OnLocalGameUnpaused;
    }

    private void GameManager_OnLocalGameUnpaused(object sender, System.EventArgs e)
    {
        bool playSound = stoveCounter.IsFrying() || stoveCounter.IsFried();

        if (playSound) { 
            audioSource.Play();
        }
    }

    private void GameManager_OnLocalGamePaused(object sender, System.EventArgs e)
    {
        audioSource.Pause();
    }

    private void SoundManager_OnSoundEffectsVolumeChanged(object sender, SoundManager.OnSoundEffectsVolumeChangedEventArgs e)
    {
        audioSource.volume = e.soundEffectsVolume;
    }

    private void Update()
    {
        //Only show the burning warning sound if the object is already fried and the burn timer is nearing completion
        if (!playWarningSound)
        {
            return;
        }

        warningSoundTimer -= Time.deltaTime;
        if(warningSoundTimer <= 0f)
        {
            float warningSoundTimerMax = .2f;
            warningSoundTimer = warningSoundTimerMax;

            SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
        }
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        //Check if the object is close to burning
        float burnShowProgressAmount = .5f;
        playWarningSound = stoveCounter.IsFried() && (e.progressNormalized >= burnShowProgressAmount);
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        //Only play the cooking sound if the object is either frying or is fried
        bool playSound = e.cookingState == StoveCounter.State.Frying || e.cookingState == StoveCounter.State.Fried;

        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }
}
