using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;    

public class Management_GameMenus : MonoBehaviour
{
    private GameObject canvasHolder;
    [SerializeField] private GameObject[] Menus;
    
    [SerializeField]
    private GameObject loadingScreen;

    private GameObject canvas_GameUI;

    [SerializeField] private GameObject canvas_Dialogue;

    private Slider loadingBar;

    [SerializeField] private GameObject firstGameOverOBJ;
    [SerializeField] private GameObject firstGameWonOBJ;
    
    
    void Awake()
    {
        canvasHolder = GameObject.Find("UI_Canvases");
        Menus = new GameObject[canvasHolder.transform.childCount]; // Broken doesnt actually add the children to the array, just empty objects.
        for (int i = 0; i < Menus.Length; i++)
        {
            Menus[i] = canvasHolder.transform.GetChild(i).gameObject;
            if(Menus[i].gameObject.activeInHierarchy){ Menus[i].SetActive(false); }
            if (Menus[i].gameObject.CompareTag("Gameplay_Canvas")) 
            { 
                Menus[i].SetActive(true);
            }
        }
        
        loadingScreen = canvasHolder.transform.Find("Canvas_LoadingScreen").gameObject;
        loadingBar = loadingScreen.transform.GetComponentInChildren<Slider>();
            
    }
    
    private void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsyncScene(sceneIndex));
        for (int i = 0; i < Menus.Length; i++)
        {
            if(Menus[i] == loadingScreen) { continue; }
            Menus[i].SetActive(false);
        }
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

    #region ----- GameScreens -----
    public void showGameOverScreen()
    {
        Time.timeScale = 0;
        canvasHolder.transform.Find("Canvas_GameOver").gameObject.SetActive(true);
        enableMouseFeatures();

        EventSystem.current.SetSelectedGameObject(firstGameOverOBJ);
    }

    public void showDialogue()
    {
        canvas_Dialogue.SetActive(true);
        FindFirstObjectByType<DialogueManager>().EnterDialogueMode(false);
    }

    public void showGameWonScreen()
    {
        Time.timeScale = 0;
        canvasHolder.transform.Find("Canvas_GameSuccess").gameObject.SetActive(true);
        enableMouseFeatures();

        EventSystem.current.SetSelectedGameObject(firstGameWonOBJ);
    }

    public void RetryLevel()
    {
        Time.timeScale = 1;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadLevel(currentSceneIndex);
        Debug.Log("Clicked On");

        EventSystem.current.SetSelectedGameObject(null);

    }

    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion
}
