using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

    [System.Serializable]
    public class Sound
    {
        public AudioClip sound;

        [Tooltip("Lower number = higher priority")] 
        [Range(0f, 256)] public int priority;
        [Range(0f, 1)] public float volume;
        [Range(0.1f, 3f)] public float pitch;
        public bool loop;
        public bool mute;
        public AudioMixerGroup mixer;
        private bool isPlaying;

        [HideInInspector] public AudioSource source;
    }

