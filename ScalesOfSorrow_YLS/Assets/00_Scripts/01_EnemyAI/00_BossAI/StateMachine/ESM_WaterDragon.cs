using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESM_WaterDragon : EnemyStateMachine
{
    protected override void BasicAttack()
    {
        Debug.Log("Water Default Attack");
    }

    protected override void RangedAttack()
    {
        base.RangedAttack();
    }

    protected override void SpecialAttack()
    {
        base.SpecialAttack();
    }

}
