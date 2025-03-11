using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private Management_GameMenus GameMenus_Manager;

    private void OnEnable()
    {
        EnemyStateMachine.OnDragonDefeated += GameWon;
        PlayerDeath.OnPlayerDefeated += GameOver;
    }

    private void OnDisable()
    {
        EnemyStateMachine.OnDragonDefeated -= GameWon;
        PlayerDeath.OnPlayerDefeated -= GameOver;
    }
    
    void Awake()
    {
        GameMenus_Manager = GetComponent<Management_GameMenus>();
    }

    void GameWon()
    {
        GameMenus_Manager.showGameWonScreen();
    }

    void GameOver()
    {
        GameMenus_Manager.showGameOverScreen();
    }
}
