using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
{

    public string soundName;

    public AudioClip sound;

    [Range(0f, 1)] public float volume;
    [Range(0.1f, 3f)] public float pitch;
    public bool loop;
    public AudioMixerGroup mixer;
    private bool isPlaying;

    [HideInInspector] public AudioSource source;
}
