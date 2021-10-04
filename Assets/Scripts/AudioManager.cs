using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager Instance;
    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        foreach(Sound s in sounds){
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.priority = s.priority;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void PlayDelayed(string name, float delay = 0){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null){
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.volume = s.volume;
        s.source.PlayDelayed(delay);
    }

    public void StopAllExcept(string[] names = null, float duration = 0){
        if(names == null) names = new string[]{""};
        foreach(Sound s in sounds){
            if(!names.Contains(s.name)){
                if(s.source.isPlaying){
                    StartCoroutine(LerpFunction(s.source,0,duration));
                }
            }
        }
    }

    public bool IsPlaying(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.isPlaying;
    }
    
    public void Stop(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public float GetClipLength(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.clip.length;
    }

    public void IncreaseVolume(string name, float endVolume, float duration){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        StartCoroutine(LerpVolume(s.source,endVolume,duration));
    }
    
    IEnumerator LerpFunction(AudioSource source,float endValue, float duration)
    {
        float time = 0;
        float startValue = source.volume;

        while (time < duration)
        {
            source.volume = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = endValue;
        source.Stop();
    }

    public Sound PlayFromGroupDelayedReturnSound(string group, int index, float delay = 0)
    {
        Sound[] soundGroup = Array.FindAll(sounds, sound => sound.groupName == group);
        if(soundGroup == null){
            Debug.LogWarning("Group: " + name + " not found!");
            return null;
        }
        Sound sound = soundGroup[index];
        sound.source.volume = sound.volume;
        sound.source.PlayDelayed(delay);
        return sound;
    }

    public int SoundGroupLength(string group)
    {
        Sound[] soundGroup = Array.FindAll(sounds, sound => sound.groupName == group);
        return soundGroup.Length;
    }

    IEnumerator LerpVolume(AudioSource source, float endValue, float duration)
    {
        float time = 0;
        float startValue = source.volume;

        while (time < duration)
        {
            source.volume = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = endValue;
    }

    public string GetArtistName(string group, int index)
    {
        Sound[] soundGroup = Array.FindAll(sounds, sound => sound.groupName == group);
        if(soundGroup == null){
            Debug.LogWarning("Group: " + name + " not found!");
            return null;
        }
        Sound sound = soundGroup[index];
        return sound.artistName;
    }

    public string GetSoundName(string group, int index)
    {
        Sound[] soundGroup = Array.FindAll(sounds, sound => sound.groupName == group);
        if(soundGroup == null){
            Debug.LogWarning("Group: " + name + " not found!");
            return null;
        }
        Sound sound = soundGroup[index];
        return sound.name;
    }
}
