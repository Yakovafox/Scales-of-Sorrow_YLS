using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI_Management : MonoBehaviour
{
    private Slider EnemyHealthBar;

    private float maxDamage;
    private float currentDamage = 0;
    public float Acc_maxDamage
    {
        set { maxDamage = value; }
    }

    void Awake()
    {
        EnemyHealthBar = GetComponentInChildren<Slider>();
    }

    void Update()
    {
        
    }

    public void updateEnemyHealthBar(float damageRecieved)
    {
        currentDamage += damageRecieved;
        float convertedDamage = Mathf.InverseLerp(0, maxDamage, currentDamage);
        EnemyHealthBar.value = convertedDamage;
    }
}
