using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public event EventHandler<OnSoundEffectsVolumeChangedEventArgs> OnSoundEffectsVolumeChanged;
    public class OnSoundEffectsVolumeChangedEventArgs : EventArgs
    {
        public float soundEffectsVolume;
    }

    [SerializeField] private AudioClipReferencesSO audioClipReferencesSO;
    [SerializeField] private Transform deliveryCounterSoundSource;

    private const string PlayerPrefsSoundEffectsVolume = "SoundEffectsVolume";
    public const float SoundEffectsVolumeLevelMax = 100f;

    private float volume = 1f;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("Sound Manager instance has already been created.");
        }
        Instance = this;

        volume = PlayerPrefs.GetFloat(PlayerPrefsSoundEffectsVolume, 1f);
    }

    private void Start()
    {
        OptionsUI.Instance.OnSoundEffectsSliderChanged += OptionsUI_OnSoundEffectsSliderChanged;
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeCompleted;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.OnAnyItemPickUp += Player_OnItemPickUp;
        BaseCounter.OnItemDropped += Counter_OnItemDropped;
        TrashCounter.OnItemThrownOut += TrashCounter_OnItemThrownOut;
    }

    private void OptionsUI_OnSoundEffectsSliderChanged(object sender, OptionsUI.OnSliderChangedEventArgs e)
    {
        //Normalizing the volume since slider goes 0-100 but AudioSource.volume goes 0-1
        volume = e.sliderValue / SoundEffectsVolumeLevelMax;

        OnSoundEffectsVolumeChanged?.Invoke(this, new OnSoundEffectsVolumeChangedEventArgs
        {
            soundEffectsVolume = volume
        });

        PlayerPrefs.SetFloat(PlayerPrefsSoundEffectsVolume, volume);
        PlayerPrefs.Save();
    }

    private void TrashCounter_OnItemThrownOut(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipReferencesSO.trash, trashCounter.transform.position);
    }

    private void Counter_OnItemDropped(object sender, System.EventArgs e)
    {
        BaseCounter counter = sender as BaseCounter;
        PlaySound(audioClipReferencesSO.objectDropped, counter.transform.position);
    }

    private void Player_OnItemPickUp(object sender, System.EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(audioClipReferencesSO.objectPickedup, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipReferencesSO.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipReferencesSO.deliveryFail, deliveryCounterSoundSource.position);
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        PlaySound(audioClipReferencesSO.deliverySuccess, deliveryCounterSoundSource.position);
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume * volumeMultiplier);
    }

    public void PlayFootstepsSound(Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipReferencesSO.footsteps, position, volume);
    }
    
    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(audioClipReferencesSO.warning, position);
    }
    
    public void PlayCountdownSound()
    {
        PlaySound(audioClipReferencesSO.warning, Vector3.zero);
    }

    public float GetVolume()
    {
        return volume;
    }
}
