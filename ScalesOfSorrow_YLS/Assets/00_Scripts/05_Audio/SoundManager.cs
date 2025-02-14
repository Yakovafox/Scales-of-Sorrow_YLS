using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instanceSM;

    [SerializeField] private GameObject soundObject;
    private void Awake()
    {
        if (instanceSM == null) { instanceSM = this; }
        else { Destroy(instanceSM); }
    }

    public void PlaySound(AudioClip audioClip, Vector3 spawnPosition, bool loops)
    {
        AudioSource audioSource = Instantiate(soundObject, spawnPosition, Quaternion.identity).GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = 1;
        audioSource.loop = loops;
        audioSource.Play();
        
        float clipLength = audioSource.clip.length;

        if (!loops){ Destroy(audioSource, clipLength); }
    }

}
