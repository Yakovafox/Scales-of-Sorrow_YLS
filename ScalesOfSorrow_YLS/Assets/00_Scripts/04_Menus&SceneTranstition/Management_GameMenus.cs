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

    private bool loading = false;
    private Image loadingBar;

    [SerializeField] private GameObject firstGameOverOBJ;
    [SerializeField] private GameObject firstGameWonOBJ;
    [SerializeField] private GameObject firstPauseOBJ;
    [SerializeField] private GameObject firstSettingsOBJ;

    private DialogueManager dialogueManager;
    
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
        loadingBar = loadingScreen.transform.GetChild(3).GetChild(0).GetComponent<Image>();
        loading = false;

        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
    }
    
    public void LoadLevel(int sceneIndex)
    {
        loading = true;

        if(sceneIndex > SceneManager.sceneCount +1)
        {
            StartCoroutine(LoadAsyncScene(0));
        }
        else { StartCoroutine(LoadAsyncScene(sceneIndex)); }

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
            loadingBar.fillAmount = loadProgress;
            
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

    public void pauseGame()
    {
        if (dialogueManager.dialogueIsPlaying || loading) { return; }
        bool paused = canvasHolder.transform.GetChild(5).gameObject.activeInHierarchy;
        canvasHolder.transform.GetChild(5).gameObject.SetActive(!paused);
        
        enableMouseFeatures();
        EventSystem.current.SetSelectedGameObject(firstPauseOBJ);

        switch (paused)
        {
            case true:
                Time.timeScale = 1;
                break;
            case false:
                Time.timeScale = 0;
                break;
        }
    }

    public void settingsButton()
    {
        canvasHolder.transform.GetChild(5).gameObject.SetActive(false);
        canvasHolder.transform.GetChild(6).gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSettingsOBJ);
    }

    public void backButton()
    {
        canvasHolder.transform.GetChild(6).gameObject.SetActive(false);
        canvasHolder.transform.GetChild(5).gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstPauseOBJ);
    }

    public void ExitGame()
    {

        canvasHolder.transform.Find("Canvas_GameSuccess").gameObject.SetActive(false);
        canvasHolder.transform.Find("Canvas_GameOver").gameObject.SetActive(true);
        StartCoroutine(LoadAsyncScene(0));
    }
    #endregion
}
