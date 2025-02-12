using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        Debug.Log("LoadScene");
    }

    public void SettingsButton()
    {

        Debug.Log("Shwo Settings");
        //load or show settings menu
    }

    public void CreditsButton()
    {

        Debug.Log("SHow Credits");
        //load or show credits menu
    }

    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
