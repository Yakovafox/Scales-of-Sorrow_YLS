using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;    

public class Management_GameMenus : MonoBehaviour
{
    private GameObject canvasHolder;
    [SerializeField] private GameObject[] Menus;
    
    [SerializeField]
    private GameObject loadingScreen;

    private Slider loadingBar;
    
    
    void Awake()
    {
        canvasHolder = GameObject.Find("UI_Canvases");
        Menus = new GameObject[canvasHolder.transform.childCount]; // Broken doesnt actually add the children to the array, just empty objects.
        for (int i = 0; i < Menus.Length; i++)
        {
            Menus[i] = canvasHolder.transform.GetChild(i).gameObject;
            if(Menus[i].gameObject.activeInHierarchy){ Menus[i].SetActive(false); }
        }
        
        loadingScreen = canvasHolder.transform.Find("Canvas_LoadingScreen").gameObject;
        loadingBar = loadingScreen.transform.GetComponentInChildren<Slider>();
            
    }
    
    private void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsyncScene(sceneIndex));
    }

    IEnumerator LoadAsyncScene(int sceneIndex)
    {
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        Debug.Log("Activated and Loading!");
        
        disableMouseFeatures();
        loadingScreen.SetActive(true);

        while (!loadAsyncOperation.isDone)
        {
            float loadProgress = Mathf.Clamp01(loadAsyncOperation.progress / 0.9f);
            loadingBar.value = loadProgress;
            
            yield return null;
        }
    }

    private void enableMouseFeatures()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void disableMouseFeatures()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void showGameOverScreen()
    {
        Time.timeScale = 0;
        canvasHolder.transform.Find("Canvas_GameSuccess").gameObject.SetActive(true);
        enableMouseFeatures();
    }

    public void showGameWonScreen()
    {
        Time.timeScale = 0;
        canvasHolder.transform.Find("Canvas_GameSuccess").gameObject.SetActive(true);
        enableMouseFeatures();
    }

    public void RetryLevel()
    {
        Time.timeScale = 1;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadLevel(currentSceneIndex);
        Debug.Log("Clicked On");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
}
