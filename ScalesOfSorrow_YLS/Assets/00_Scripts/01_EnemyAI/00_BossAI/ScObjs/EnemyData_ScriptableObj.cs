using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "newEnemyData", menuName = "Scriptable Objects/Enemy Data Scriptable Object", order = 3)]
public class EnemyData_ScriptableObj : ScriptableObject
{


    [Header("------ General Values ------")]
    [Tooltip("Specific ID refering to the specific enemy. Used for code to search for specific enemies.")]
    public int ID = 0;
    [Tooltip("The assigned name of the character.")]
    public string dragonName;
    [Tooltip("Reference to enemy Prefab.")]
    public GameObject Dragon_Reference;

    [Tooltip("What the character should chase")]
    public List<GameObject> Target;

    //----------------------------------------------------------------------------------

    [Header("------ Movement Variables ------")]
    [Header("Speed Variables")]

    [Tooltip("The speed at which the character will walk/patrol.")]
    public float walkSpeed = 2f;

    [Tooltip("Can the dragon lift off from the ground?")]
    public bool canFly = false;

    [Tooltip("The speed at which the character will sprint.")]
    public float flySpeed = 4f;

    [Tooltip("Boolean for whether or not the character is sprinting.")]
    public bool flying = false;

    [Tooltip("The radius of the circle in which the enemy can move within.")]
    public float moveRadius = 5f;

    [Header("Cooldown Variables")]

    [Tooltip("Time to wait before continuing the character's process. (This could be for an interaction or a time to wait while searching for the player.)")]
    public float timeToWait;

    [Tooltip("Reference to the ground layer that the character should use.")]
    public LayerMask GroundLayer;

    //---------------------------------------------------------------------------------

    [Header("------ Character's Senses ------")]
    [Header("Sight Variables")]

    [Tooltip("Boolean that defines whether the character can see or not.")]
    public bool canSee = true;
    [Tooltip("The Distance at which the character can spot players.")]
    public float sightDistance = 10f;
    [Tooltip("The angle at which the character can see others.")]
    [Range(0.0f, 360f)]
    public float sightAngle = 120f;

    //----------------------------------------------------------------------------------

    [Header("----- My Attributes -----")]

    [Tooltip("The maximum health of the enemy.")]
    [Range(0, 1000)]
    public float MaxHealth = 100;

    [Tooltip("The stages amount of times the player needs to rain the max health from the enemy.")]
    [Range(0, 3)]
    public int Stages = 1;

    [Header("Flying Variables")] [Tooltip("The desired amount of time that the dragon should spend in the air.")]
    public float flightTime = 5f;
    [Tooltip("The cooldown time that determines how long the before the dragon can fly again.")]
    public float flightCooldownTime = 10f;
    [Tooltip("Time to wait when being stunned/Time that dragon will spend before moving after landing.")]
    public float stunTime = 1f;
    
    [Tooltip("The chance at which the dragon will take flight.")]
    [Range(0,100)]
    public float chanceToFly = 33.33f;

    [Tooltip("The distance at which the push back will search for a player, when dragon is landing.")]
    public float pushBackDistance = 5f;
    [Tooltip("The amount of force applied to the player on a dragons landing pushback function.")]
    public float pushBackAmount = 5f;
    
    [Tooltip("The default size of the dragon's shadow when on the floor.")]
    public float shadow_DefaultSize = 3f;
    
    [Tooltip("The maximum size of the dragon's shadow when off the floor.")]
    public float shadow_MinSize = 1f;

    //---------------------------------------------------------------------------------

    [Header("------ Attack Variables ------")]
    [Header("General Attack Variables")]
    
    [Tooltip("Cooldown before Range attack can be performed again.")]
    public float attackCooldown = 2.5f;
    
    [Header("Default Attack")]

    [Tooltip("The maximum distance that the player can be for a melee attack. NOTE: MUST be a value greater than 0!")]
    public float meleeAttackDistance;
    [Tooltip("The maximum melee damage the enemy can deal.")]
    [Range(0,100)]
    public float meleeDamage = 10f;

    [Header("Ranged Attack")]
    [Tooltip("The maximum distance that the player can be for a melee attack. NOTE: MUST be a value greater than 0!")]
    public float rangedAttackDistance;
    [Tooltip("The maximum ranged damage the enemy can deal.")]
    [Range(0, 100)]
    public float rangedDamage = 10f;
    [Tooltip("The projectile for a ranged attack.")]
    public GameObject rangedProjectile;

    //-------------------------------------------------------------------------------
    
    [Header("----- Special Variables -----")]
    
    [Header("Special Attack")]
    [Tooltip("The chance that a special attack will happen.")]
    [Range(0, 100)]
    public float specialAttackChance = 33.33f;
    [Tooltip("Damage that the special attack should deal.")]
    [Range(0, 150)]
    public float SpecialDamage;
    [Tooltip("Time that the Dragon stays in the ability.")]
    public float ability_Timer = 10f;
    [Tooltip("Cooldown time before the Dragon can enter the special ability again.")]
    public float ability_cooldownTime = 15f;

    [Header("-- Individual Special Variables --")]

    [Tooltip("Shield prefab that the dragon can put up in the Special State.")]
    public GameObject Shield;
    
    [Tooltip("Time before dragon can Dash again.")]
    public float dashCooldownTime = 2.25f;
    [Tooltip("Trap prefab that the dragon can deploy in the Special State.")]
    public GameObject Trap;

    [Tooltip("Multiplied by default damage, used when in dragons special ability.")]
    public float fireup_DamageMultiplier = 1.5f;
    [Tooltip("Time it takes for the dragon to gain damage buff.")]
    public float fireup_ChargeTime = 3f;

}
