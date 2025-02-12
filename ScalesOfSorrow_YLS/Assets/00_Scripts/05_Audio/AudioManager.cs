using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager audioManagerInstnace;
    public Sound[] sounds;

    private void Awake()
    {
        if (audioManagerInstnace != null && audioManagerInstnace != this) { Destroy(this); }
        else { audioManagerInstnace = this; }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.sound;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixer;
        }
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        if (!FindFirstObjectByType<AudioManager>().IsPlaying("Music")) { FindFirstObjectByType<AudioManager>().PlaySound("Music"); }
    }
    public void PlaySound(string soundName)
    {
        Sound s = Array.Find(sounds, Sound => Sound.soundName == soundName);
        Debug.Log("Playing Sound: " + s.soundName);
        s.source.Play();
    }
    public void StopAudio(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.soundName == soundName);
        s.source.Stop();
    }

    public bool IsPlaying(string soundName)
    {
        Sound s = Array.Find(sounds, Sound => Sound.soundName == soundName);
        if (s.source.isPlaying) { return true; }
        else { return false; }
    }
}
