using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private GameObject loadingCanvas;

    [Header("First Selected Objects")]
    [SerializeField] private GameObject mainMenuFirstSelect;
    [SerializeField] private GameObject settingsMenuFirstSelect;
    [SerializeField] private GameObject creditsFirstSelect;

    private Slider loadingBar;

    private void Start()
    {
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        loadingCanvas.SetActive(false);

        loadingBar = loadingCanvas.GetComponentInChildren<Slider>();

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
        mainMenuCanvas.SetActive(false);
        StartCoroutine(LoadAsyncScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadAsyncScene(int sceneIndex)
    {
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        Debug.Log("Activated and Loading!");

        loadingCanvas.SetActive(true);

        while (!loadAsyncOperation.isDone)
        {
            float loadProgress = Mathf.Clamp01(loadAsyncOperation.progress / 0.9f);
            loadingBar.value = loadProgress;

            yield return null;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

}