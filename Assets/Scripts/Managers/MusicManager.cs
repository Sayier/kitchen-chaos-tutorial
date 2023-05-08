using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string PlayerPrefsMusicVolume = "MusicVolume";

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

    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0f;
        }
        audioSource.volume = volume;

        PlayerPrefs.SetFloat(PlayerPrefsMusicVolume, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
