using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingBehaviour : MonoBehaviour
{
    public static SettingBehaviour settingBehaviourInstance;

    [SerializeField] private Input Controls; 
    [SerializeField] private PlayerControls playerControls; 

    [SerializeField] private Canvas settingsMenu;
    [SerializeField] private bool menuEnabled = false;
    private void Awake()
    {
        if (settingBehaviourInstance != null && settingBehaviourInstance != this) { Destroy(this); }
        else { settingBehaviourInstance = this; }
        DontDestroyOnLoad(gameObject);

        SettingsMenu(false);
    }
    // Update is called once per frame
    void Update()
    {

            menuEnabled = !menuEnabled;
            SettingsMenu(menuEnabled);
        
    }

    public void SettingsMenu(bool menuEnabled)
    {
        if (menuEnabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            settingsMenu.enabled = true;
            Time.timeScale = 0f;
        }
        else if (!menuEnabled)
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") 
            { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            }
            settingsMenu.enabled = false;
            Time.timeScale = 1f;
        }
    }
}
