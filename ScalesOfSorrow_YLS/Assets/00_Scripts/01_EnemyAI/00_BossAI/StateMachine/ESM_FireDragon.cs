using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ESM_FireDragon : EnemyStateMachine
{

    protected override void givePlayerSpecialAbility()
    {
        StartCoroutine(enableDisableAbilityUnlock());

        for (int i = 0; i < PlayerRef.Count; i++)
        {
            PlayerRef[i].GetComponent<PlayerController>().Acc_upgradeFiredUp = true;
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
        if (!audioSource.isPlaying)
        {
            Luke_SoundManager.PlaySound(SoundType.DragonFireupSpecial, 1, audioSource);
        }
        if (!firedUp)
        {
            StartCoroutine(SpriteFlasher(myData_SO.fireup_ChargeTime, myData_SO.FireUp_Colour, myData_SO.FireUp_AnimCurve));
            yield return new WaitForSeconds(myData_SO.fireup_ChargeTime);
            firedUp = true;
        }
        yield return null;
    }

}
