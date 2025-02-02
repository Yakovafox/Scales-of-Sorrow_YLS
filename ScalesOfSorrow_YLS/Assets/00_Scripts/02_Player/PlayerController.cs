using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

public class PlayerController : MonoBehaviour
{
    #region Varriables
    [Header("------- Health -------")]
    [SerializeField] private float health;

    [Header("------- Movement -------")]
    [SerializeField] private float pSpeed; 
    private Vector2 movementInput = Vector2.zero;
    private Vector3 rotation;
    private Transform pTransform;
    private Rigidbody pRB;
    private SpriteRenderer pSR;
  
    [Header("------- Dash -------")]
    [Tooltip("dashSpeed controls how fast with the player moves during the dash. This can be used to control the distance")]
    [SerializeField] private float dashSpeed;
    private Collider pCollider;
    private bool canDash = true;
    private bool isDash = false;

    [Tooltip("dashTime controls how long the dash lasts")]
    [SerializeField] private float dashTime;

    [Tooltip("dashTime controls how often the player can dash")]
    [SerializeField] private float dashCooldown;

    [Header("------- Attack -------")]
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
                     private bool canAttack = true;

    [Header("------- Fired Up -------")]
    [Tooltip("Controls the extra damage for the fired up ability")]
    [SerializeField] private float extraDamage;

    [Tooltip("Controls fired up Max Duration")]
    [SerializeField] private float firedUpDuration;

    [Tooltip("Controls fired up's cooldown")]
    [SerializeField] private float firedUpCooldown;
    private bool isFiredUp = false;

    [Header("------- Shield -------")]
    [SerializeField] private GameObject shieldPrefab;  
                     private GameObject shieldReference;
    [SerializeField] private float shieldDuration;
    [SerializeField] private float shieldCooldown;
    [SerializeField] private float shieldDistance;
    private bool isShield = false;
    private bool shieldExists = false;
    private bool shieldMove = false;

    [Header("------- Upgrades -------")]
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
        if (!shieldMove) { MoveInput(); }
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

    private Vector3 GetRotate()
    {

        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
        Vector3 rotateDirection = (new Vector3(direction.x - transform.position.x, 0, direction.z - transform.position.z)).normalized;
        float angle = Mathf.Atan2(rotateDirection.x, rotateDirection.z) * Mathf.Rad2Deg;

        Vector3 rotate = new Vector3(0, angle, 0);
        return rotate;
    }
    #endregion

    #region Dash -------------------------------------------------------------------------------
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
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        attackCharges -= 1;

        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);

        RaycastHit[] hits = Physics.BoxCastAll(pTransform.position, new Vector3(attackSize, attackSize, attackSize), direction, quaternion.Euler(GetRotate()), attackRange, attackMask, PreviewCondition.Both, 1f, Color.green, Color.red);

        //play particle effect
        //SoundEffect

        for (int i = 0; i < hits.Length; i++) 
        {
            if (isFiredUp)
            {
                //damage uses extra damaage
            }

            //Damage Enemies
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    #region Fired Up ----------------------------------------------------------------------------------------------------------------
    public void OnFiredUp(InputAction.CallbackContext context)
    {
        if (upgradeFiredUp && context.started) 
        {
            StopCoroutine(fireUp());
            StartCoroutine(fireUp());
        }
    }

    private IEnumerator fireUp()
    {
        Debug.Log("Pre Fired Up: " + isFiredUp);
        isFiredUp = !isFiredUp;
        Debug.Log("post Fired Up: " + isFiredUp);

        yield return new WaitForSeconds(firedUpDuration);

        isFiredUp = false;

        Debug.Log("(false) Fired Up: " + isFiredUp);

    }

    #endregion

    #region Shield --------------------------------------------------------------------------------------------------

    public void OnShield(InputAction.CallbackContext context)
    {
        if (upgradeShield && context.started)
        {
            StopCoroutine(Shield());
            StartCoroutine(Shield());
        }
    }

    IEnumerator Shield()
    {
        isShield = !isShield;

        if (isShield && !shieldExists)
        {
            Vector3 spawn = pTransform.position + (new Vector3(movementInput.x, 0, movementInput.y) * shieldDistance);
            shieldReference = Instantiate(shieldPrefab, spawn, quaternion.identity, transform);
            shieldReference.transform.LookAt(pTransform.position);
            shieldExists = true;
        }
        if (shieldExists == true) { shieldMove = true; }

        yield return new WaitForSeconds(shieldDuration);

        shieldMove = false;
        if(shieldReference != null) { Destroy(shieldReference); shieldExists = false; }

        yield return new WaitForSeconds(shieldCooldown);
        isShield = false;
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
