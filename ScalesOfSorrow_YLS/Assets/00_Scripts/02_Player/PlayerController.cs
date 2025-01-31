using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

public class PlayerController : MonoBehaviour
{
    #region Varriables
    [Header("Lives")]
    [SerializeField] private float health;

    //UI Variables
    [Header("Movement")]
    [SerializeField] private float pSpeed;
    [SerializeField] private Vector2 movementInput = Vector2.zero;
    private Transform pTransform;
    private Rigidbody pRB;
    private SpriteRenderer pSR;
  
    [Header("Dash")]
    [Tooltip("dashSpeed controls how fast with the player moves during the dash. This can be used to control the distance")]
    [SerializeField] private float dashSpeed;
    private Collider pCollider;
    private bool canDash;
    private bool isDash;

    [Tooltip("dashTime controls how long the dash las")]
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    [Header("Attack")]
    [Tooltip ("Controls which layers can take damage")]
    [SerializeField] private LayerMask attackMask;

    [Tooltip("Controls how far the attack boxCast goes - keep low")]
    [SerializeField] private float attackRange;
    [Tooltip("Controls how the size of the box cast - value is halved ")] 
    [SerializeField] private float attackSize;

    [Tooltip("Controls time between attacks")]
    [SerializeField] private float attackCooldown;

    [Tooltip("Controls amount of damage dealt")]
    [SerializeField] private float attackDamage;

    [Tooltip("Controls amount of attack charges")]
    [SerializeField] private int maxCharges;
                     private int attackCharges;

    private bool canAttack;

    [Header("Upgrades")]
    [SerializeField] private bool upgradeShield;
    [SerializeField] private bool upgradeFiredUp;
    [SerializeField] private bool upgradeDash;

    #endregion
    void Start()
    {
        pTransform = transform;
        pRB = GetComponent<Rigidbody>();
        pCollider = GetComponent<Collider>();
        pSR = GetComponentInChildren<SpriteRenderer>();

        attackCharges = maxCharges;
    }

    void Update()
    {
        if (isDash) { return; }
    }

    private void FixedUpdate()
    {
        if (isDash) { return; }
        MoveInput();
    }

    #region Movement -----------------------------------------------------------------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    
    private void MoveInput()
    {
        Vector3 axis = new Vector3(movementInput.x, 0, movementInput.y);
        pTransform.position += (axis.normalized * pSpeed * Time.deltaTime);

        if(axis.x > 0) { pSR.flipX = false; }
        else if (axis.x < 0) {pSR.flipX = true; }
    }
    #endregion

    #region Dash
    public void OnDash(InputAction.CallbackContext context)
    {
        /*if (context.started && canDash && upgradeDash ) {  Upgraded Dash }
        else if (context.started && canDash  ) { StartCoroutine(DefaultDash()); }*/

        if (!canDash || !context.started) { return; }
        else if (upgradeDash) { /* Upgraded Dash */ }
        else { StartCoroutine(DefaultDash()); }

    }

    private IEnumerator DefaultDash()
    {
        pCollider.enabled = false;
        canDash = false;
        isDash = true;

        pRB.velocity = new Vector3(movementInput.x, 0, movementInput.y) * dashSpeed;

        yield return new WaitForSeconds(dashTime);
        pCollider.enabled = true;
        isDash = false;
        pRB.velocity = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    #endregion

    #region Attack --------------------------------------------------------------------------------------------------------------------------

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || attackCharges <= 0) { return; }
        else { StartCoroutine(Attack()); }
        Debug.Log("Attack");
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        attackCharges -= 1;

        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
        Vector3 rotateDirection = (new Vector3(direction.x - transform.position.x, 0, direction.z - transform.position.z)).normalized;
        float angle = Mathf.Atan2(rotateDirection.x, rotateDirection.z) * Mathf.Rad2Deg;

        Vector3 rotate = new Vector3(0, angle, 0);
        RaycastHit[] hits = Physics.BoxCastAll(pTransform.position, new Vector3(attackSize, attackSize, attackSize), direction, quaternion.Euler(rotate), attackRange, attackMask, PreviewCondition.Both, 1f, Color.green, Color.red);

        //play particle effect
        //SoundEffect

        for (int i = 0; i < hits.Length; i++) 
        {
            //Damage Enemies
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    #region Collision -----------------------------------------------------------------------------------------------------------------
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health >= 0) { /*reload current scene*/ }
    }

    public void RechargeMelee()
    {
        attackCharges++;
    }

    #endregion 
}
