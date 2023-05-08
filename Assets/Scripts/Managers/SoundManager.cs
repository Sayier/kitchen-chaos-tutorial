using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string PlayerPrefsSoundEffectsVolume = "SoundEffectsVolume";

    [SerializeField] private AudioClipReferencesSO audioClipReferencesSO;
    [SerializeField] private Transform deliveryCounterSoundSource;
    
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
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.Instance.OnItemPickUp += Player_OnItemPickUp;
        BaseCounter.OnItemDropped += Counter_OnItemDropped;
        TrashCounter.OnItemThrownOut += TrashCounter_OnItemThrownOut;
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
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume * volumeMultiplier);
    }

    public void PlayFootstepsSound(Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipReferencesSO.footsteps, position, volume);
    }

    //Increase volume by 10%, reset to 0 if over 100%
    public void ChangeVolume()
    {
        volume += .1f;
        if(volume > 1f)
        {
            volume = 0f;
        }

        PlayerPrefs.SetFloat(PlayerPrefsSoundEffectsVolume, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
