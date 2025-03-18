using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private Management_GameMenus GameMenus_Manager;

    float numOfDialogues = 2;
    float dialoguesExecuted = 0;

    private void OnEnable()
    {
        EnemyStateMachine.OnDragonDefeated += GameWon;
        DialogueManager.OnEndedDialogue += unlockMoveToNextLevel;
        PlayerDeath.OnPlayerDefeated += GameOver;
    }

    private void OnDisable()
    {
        EnemyStateMachine.OnDragonDefeated -= GameWon;
        DialogueManager.OnEndedDialogue -= unlockMoveToNextLevel;
        PlayerDeath.OnPlayerDefeated -= GameOver;
    }
    
    void Awake()
    {
        Time.timeScale = 1f;
        GameMenus_Manager = GetComponent<Management_GameMenus>();
    }

    void GameWon()
    {
        GameMenus_Manager.showDialogue();
    }

    void GameOver()
    {
        GameMenus_Manager.showGameOverScreen();
    }

    private void unlockMoveToNextLevel()
    {
        dialoguesExecuted += 1;
        if(dialoguesExecuted >= numOfDialogues)
        {
            print("MOVE TO NEXT LEVEL");
            GameMenus_Manager.LoadLevel(SceneManager.GetActiveScene().buildIndex +1);
        }
    }
}
