using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using UnityEngine;

public enum SoundType
{
    PlayerDash,
    PlayerAttack,
    PlayerShield,
    PlayerHitDamage,
    PlayerHitAmmo,
    DragonMove,
    DragonFly,
    DragonMeleeAttack,
    DragonRangedAttack,
    DragonHit,
    WaterDragonSpecial,
    DragonFireupSpecial
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
    public AudioClip[] Sounds
    {
        get => sounds;
    }
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class Luke_SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static Luke_SoundManager instance;
    private AudioSource default_audioSource;
    private AudioSource referenced_audioSource;

#if UNITY_EDITOR
    
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
    private void Awake()
    {
        instance = this;
        default_audioSource = GetComponent<AudioSource>();
    }
    
    public static void PlaySound(SoundType sound, float volume = 1, AudioSource audioSource = null)
    {
        instance.referenced_audioSource = audioSource;

        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        instance.referenced_audioSource.PlayOneShot(randomClip, volume);
    }
}
