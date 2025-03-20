using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private Canvas loadingScreen;
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas creditsCanvas;
    [Header("Loading")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Animation animationBar;
    [SerializeField] private string sceneName;
    [Header("Settings")]
    [SerializeField] private SettingBehaviour settingBehaviour;

    //is enabled
    private bool creditsEnabled = false;
    private bool mainEnabled = true;
    private void Start()
    {
        animationBar.Play("anime-loading");
        animationBar["anime-loading"].speed = 0;
    }

    public void PlayButton()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        loadingScreen.enabled = true;
        mainMenu.enabled = false;
        StartCoroutine(LoadAsync(sceneName));
        Debug.Log("LoadScene");
    }

    private IEnumerator LoadAsync(string path)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(path);

        while (!operation.isDone) 
        {
            float progress = Mathf.Clamp01(operation.progress/.9f);
            progressBar.value = progress;
            progressBar.value += progress;
            animationBar["anime-loading"].normalizedTime = progressBar.value ;
        }


        yield return new WaitForSeconds(100f);
    }

    public void SettingsButton()
    {
        Debug.Log("Show Settings");
        settingBehaviour.SettingsMenu(enabled);
        mainMenu.enabled = !mainEnabled;
    }

    public void CreditsButton()
    { 
        Debug.Log("SHow Credits");
        creditsCanvas.enabled = !creditsEnabled;
        mainMenu.enabled = !mainEnabled;
        //load or show credits menu
    }

    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
