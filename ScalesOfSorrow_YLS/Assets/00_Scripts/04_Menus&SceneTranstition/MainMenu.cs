using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void SettingsButton()
    {
        //load or show settings menu
    }

    public void CreditsButton()
    {
        //load or show credits menu
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
