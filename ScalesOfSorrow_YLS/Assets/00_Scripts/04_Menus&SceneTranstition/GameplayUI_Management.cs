using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Management : MonoBehaviour
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

    public void resetEnemyHealthBar(float currentHealth, int stages)
    {
        if(currentHealth * (stages +1) == maxDamage)
        {
            EnemyHealthBar.value = 0;
            return;
        }
        currentDamage = currentHealth;
        float convertedDamage = Mathf.InverseLerp(0, maxDamage, currentDamage);
        EnemyHealthBar.value = convertedDamage;
    }

    public void updateEnemyHealthBar(float damageRecieved)
    {
        currentDamage += damageRecieved;
        float convertedDamage = Mathf.InverseLerp(0, maxDamage, currentDamage);
        EnemyHealthBar.value = convertedDamage;
    }
}
