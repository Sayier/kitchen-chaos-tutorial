using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string PlayerPrefsMusicVolume = "MusicVolume";
    public const float MusicVolumeLevelMax = 100f;

    private float volume;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Music Manager instance has already been created.");
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        volume = PlayerPrefs.GetFloat(PlayerPrefsMusicVolume, .3f);
        audioSource.volume = volume;
    }

    private void Start()
    {
        OptionsUI.Instance.OnMusicSliderChanged += OptionsUI_OnMusicSliderChanged;
    }

    private void OptionsUI_OnMusicSliderChanged(object sender, OptionsUI.OnSliderChangedEventArgs e)
    {
        //Normalizing the volume since slider goes 0-100 but AudioSource.volume goes 0-1
        volume = e.sliderValue/MusicVolumeLevelMax;
        audioSource.volume = volume;

        PlayerPrefs.SetFloat(PlayerPrefsMusicVolume, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
