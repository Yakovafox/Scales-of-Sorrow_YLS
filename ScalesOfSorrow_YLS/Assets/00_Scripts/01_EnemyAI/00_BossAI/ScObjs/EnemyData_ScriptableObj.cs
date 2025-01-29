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

    [Header("Special Attack")]
    [Tooltip("The chance that a special attack will happen.")]
    [Range(0, 100)]
    public float specialAttackChance = 33.33f;
    [Tooltip("Damage that the special attack should deal.")]
    [Range(0, 150)]
    public float SpecialDamage;

}
