using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public string groupName;
    public string artistName;
    public AudioClip clip;
    
    [Range(0,256)]
    public int priority;

    [Range(0f,1f)]
    public float volume;

    [Range(0.1f,3f)]
    public float pitch = 1;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
