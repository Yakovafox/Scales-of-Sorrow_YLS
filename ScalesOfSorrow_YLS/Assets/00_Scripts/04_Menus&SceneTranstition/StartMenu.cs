using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class StartMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject creditsCanvas;

    [Header("First Selected Objects")]
    [SerializeField] private GameObject mainMenuFirstSelect;
    [SerializeField] private GameObject settingsMenuFirstSelect;
    [SerializeField] private GameObject creditsFirstSelect;

    private void Start()
    {
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);

    }

    public void OpenMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelect);
    }

    public void OpenSettingsMenu()
    {
        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
        creditsCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(settingsMenuFirstSelect);
    }

    public void OpenCreditsMenu()
    {
        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);

        EventSystem.current.SetSelectedGameObject(creditsFirstSelect);
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }

}