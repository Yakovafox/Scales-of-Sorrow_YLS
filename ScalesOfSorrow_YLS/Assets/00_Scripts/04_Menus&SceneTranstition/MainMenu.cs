using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private SettingBehaviour settingBehaviour;
    [SerializeField] private Canvas creditsCanvas;
    private bool creditsEnabled = false;
    public void PlayButton()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        Debug.Log("LoadScene");
    }

    public void SettingsButton()
    {

        Debug.Log("Shwo Settings");
        settingBehaviour.SettingsMenu(enabled);

    }

    public void CreditsButton()
    { 
        Debug.Log("SHow Credits");
        creditsCanvas.enabled = !creditsEnabled;
        //load or show credits menu
    }

    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
