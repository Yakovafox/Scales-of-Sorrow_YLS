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

    public void SetMasterVolume(float volume) { audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20); }
    public void SetSoundEffectVolume(float volume) { audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20); }
    public void SetMusicVolume(float volume) { audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20); }
    public void Quit() { SceneManager.LoadScene(0); }
}
