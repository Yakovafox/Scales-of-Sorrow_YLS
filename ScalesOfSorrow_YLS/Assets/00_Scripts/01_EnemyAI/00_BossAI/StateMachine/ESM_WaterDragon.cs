using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ESM_WaterDragon : EnemyStateMachine
{

    protected override void givePlayerSpecialAbility()
    {
        StartCoroutine(enableDisableAbilityUnlock());

        for (int i = 0; i < PlayerRef.Count; i++)
        {
            PlayerRef[i].GetComponent<PlayerController>().Acc_upgradeShield = true;
            PlayerRef[i].GetComponent<PlayerController>().upgrade();
        }
    }  

    protected override IEnumerator BasicAttack()
    {
        yield return base.BasicAttack();
    }

    protected override IEnumerator RangedAttack()
    {
        yield return base.RangedAttack();
    }

    protected override void initialiseSpecialAbility()
    {
        animationController.SetBool("isSpecial", true);
        if (!audioSource.isPlaying)
        {
            Luke_SoundManager.PlaySound(SoundType.WaterDragonSpecial, 1, audioSource);
        }

        Debug.Log("Initialising Special Ability");
        //Setup any functionality for the ability here, spawn in shield etc.
        //In base machine setup all abilities at once 
        if (!myData_SO.Shield.IsUnityNull())
        {
            shieldRef = Instantiate(myData_SO.Shield, InstantiatePosition.transform.position, Quaternion.identity);
            shieldRef.transform.parent = InstantiatePosition.transform;
        }
    }

    protected override void exitSpecialAbility()
    {
        animationController.SetBool("isSpecial", false);

        if (!shieldRef.IsUnityNull())
        {
            Destroy(shieldRef);
        }
    }

    protected override IEnumerator specialFunctionality()
    {
        yield return base.specialFunctionality();
    }

}
