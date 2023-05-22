using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/AudioClipReferencesSO")]
public class AudioClipReferencesSO : ScriptableObject
{
    public AudioClip[] chop;
    public AudioClip[] deliveryFail;
    public AudioClip[] deliverySuccess;
    public AudioClip[] footsteps;
    public AudioClip[] objectDropped;
    public AudioClip[] objectPickedup;
    public AudioClip[] panSizzle;
    public AudioClip[] trash;
    public AudioClip[] warning;
}
