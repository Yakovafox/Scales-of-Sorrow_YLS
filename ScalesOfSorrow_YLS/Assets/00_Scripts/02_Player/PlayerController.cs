using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

public class PlayerController : MonoBehaviour
{
    #region ------------------------    Variables    ------------------------
    [Header("------- Health -------")]
    [SerializeField] private float health;

    [Header("------- Movement -------")]
    [SerializeField] private float pSpeed;
    [SerializeField] private Vector2 movementInput = Vector2.zero;
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

    [SerializeField] private LayerMask excludeLayers;
    [SerializeField] private LayerMask includeLayers;

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

    [Header("------- Audio -------")]
    [SerializeField] private AudioClip movementClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip rechargeClip;
    [SerializeField] private AudioClip noChargeClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip firedUpClip;
    [SerializeField] private AudioClip firedDownClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;


    #endregion ------------------------    Variables    ------------------------
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

    #region ------------------------    Movement    ------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

        if (movementClip != null) { SoundManager.instanceSM.PlaySound(movementClip, transform.position, false); }

    }
    
    private void MoveInput()
    {
        Vector3 axis = new Vector3(movementInput.x, 0, movementInput.y);
        pRB.velocity = (axis.normalized * (pSpeed * Time.deltaTime));

        if(axis.x > 0) { pSR.flipX = false; }
        else if (axis.x < 0) {pSR.flipX = true; }
    }

    #endregion ------------------------    Movement    ------------------------

    #region ------------------------    Dash    ------------------------
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash || !context.started) { return; }
        else 
        { 
            StartCoroutine(DefaultDash());

            if (dashClip != null) { SoundManager.instanceSM.PlaySound(dashClip, transform.position, false); }
        }
    }

    private IEnumerator DefaultDash()
    {
        pCollider.excludeLayers = excludeLayers;
        canDash = false;
        isDash = true;

        pRB.velocity = new Vector3(movementInput.x, 0, movementInput.y) * dashSpeed;

        yield return new WaitForSeconds(dashTime);
        pCollider.excludeLayers = includeLayers;
        isDash = false;
        pRB.velocity = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    #endregion ------------------------    Dash    ------------------------

    #region ------------------------    Attack    ------------------------

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || attackCharges <= 0) 
        {

            if (noChargeClip != null) { SoundManager.instanceSM.PlaySound(noChargeClip, transform.position, false); }
            return; 
        }
        else
        {
            if (attackClip != null) { SoundManager.instanceSM.PlaySound(attackClip, transform.position, false); }
            StartCoroutine(Attack()); 
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        attackCharges -= 1;

        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);

        RaycastHit[] hits = Physics.SphereCastAll(pTransform.position, attackSize, direction, attackRange, attackMask, PreviewCondition.Both, 1f, Color.green, Color.red);

        //play particle effect
        //SoundEffect

        for (int i = 0; i < hits.Length; i++) 
        {
            if (isFiredUp)
            {
                //damage uses extra damaage
            }

            if (hits[i].transform.CompareTag("Enemy"))
            {

            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion ------------------------    Attack    ------------------------

    #region ------------------------    Fired Up    ------------------------
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
        isFiredUp = !isFiredUp;

        if (isFiredUp)
        {
            if (firedUpClip != null) { SoundManager.instanceSM.PlaySound(firedUpClip, transform.position, false); }
        }
        else
        {
            if (firedDownClip != null) { SoundManager.instanceSM.PlaySound(firedDownClip, transform.position, false); }
        }

        yield return new WaitForSeconds(firedUpDuration);

        isFiredUp = false;

        Debug.Log("(false) Fired Up: " + isFiredUp);

    }

    #endregion ------------------------    Fired Up    ------------------------

    #region ------------------------    Shield    ------------------------

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
            shieldReference = Instantiate(shieldPrefab, pTransform.position, quaternion.identity, transform);
            
            shieldReference.transform.LookAt(pTransform.position);
            shieldExists = true;
        } 
        else if (!isShield && shieldExists)
        {
            Destroy(shieldReference);
            shieldMove = false;
            StopCoroutine(Shield());
        }

        if (shieldExists == true) { shieldMove = true; }

        yield return new WaitForSeconds(shieldDuration);

        shieldMove = false;
        if (shieldReference != null) { Destroy(shieldReference); shieldExists = false; }

        yield return new WaitForSeconds(shieldCooldown);
        isShield = false;
    }

    public void ShieldDestroyed()
    {
        shieldExists = false;
    }
    #endregion ------------------------    Shield    ------------------------

    #region ------------------------    Collision    ------------------------
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (hitClip != null) { SoundManager.instanceSM.PlaySound(hitClip, transform.position, false); }

        if (health >= 0) 
        {
            if (deathClip != null) { SoundManager.instanceSM.PlaySound(deathClip, transform.position, false); }
        }
    }

    public void RechargeMelee()
    {
        if (rechargeClip != null) { SoundManager.instanceSM.PlaySound(rechargeClip, transform.position, false); }
        attackCharges++;
    }

    #endregion ------------------------    Collision    ------------------------
}
