using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instanceSM;

    [SerializeField] private GameObject soundObject;
    private void Awake()
    {
        if (instanceSM == null) { instanceSM = this; }
        else { Destroy(instanceSM); }
    }

    public void PlaySound(Sound sound, Vector3 spawnPos)
    {
        AudioSource audioSource = Instantiate(soundObject, spawnPos, Quaternion.identity).GetComponent<AudioSource>();
        audioSource.clip = sound.sound;
        audioSource.priority = sound.priority;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.outputAudioMixerGroup = sound.mixer;
        audioSource.loop = sound.loop;
        audioSource.mute = sound.mute;
        audioSource.Play();
        
        float clipLength = audioSource.clip.length;

        if (!sound.loop){ Destroy(gameObject, clipLength); }
    }

}
