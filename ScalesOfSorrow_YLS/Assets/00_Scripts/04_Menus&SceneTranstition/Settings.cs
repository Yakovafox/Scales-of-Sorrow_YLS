using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
   public static Settings settingsInstance;

    [SerializeField] private AudioMixer audioMixer;
    private void Awake()
    {
        if (settingsInstance != null && settingsInstance != this) { Destroy(this); }
        else { settingsInstance = this; }
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float volume) { audioMixer.SetFloat("Master", volume); }
    public void SetSoundEffectVolume(float volume) { audioMixer.SetFloat("SFX", volume); }
    public void SetMusicVolume(float volume) { audioMixer.SetFloat("Music", volume); }
    public void Quit() { SceneManager.LoadScene(0); }
}
