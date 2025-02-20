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
    [Header("------- ID -------")]
    public int playerID;

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
    [SerializeField] private Sound movementClip;
    [SerializeField] private Sound attackClip;
    [SerializeField] private Sound rechargeClip;
    [SerializeField] private Sound noChargeClip;
    [SerializeField] private Sound dashClip;
    [SerializeField] private Sound firedUpClip;
    [SerializeField] private Sound firedDownClip;
    [SerializeField] private Sound playerHitClip;
    [SerializeField] private Sound deathClip;


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

    public void SetPlayerID(int ID) { playerID = ID; }

    #region ------------------------    Movement    ------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

        if (movementClip.sound != null) { SoundManager.instanceSM.PlaySound(movementClip, transform.position); }

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

            if (dashClip.sound != null) { SoundManager.instanceSM.PlaySound(dashClip, transform.position); }
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

            if (noChargeClip.sound != null) { SoundManager.instanceSM.PlaySound(noChargeClip, transform.position); }
            return; 
        }
        else
        {
            if (attackClip.sound != null) { SoundManager.instanceSM.PlaySound(attackClip, transform.position); }
            StartCoroutine(Attack()); 
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        attackCharges -= 1;

        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);

        RaycastHit[] hits = Physics.SphereCastAll(pTransform.position, attackSize, direction, attackRange, attackMask, PreviewCondition.Both, 1f, Color.green, Color.red);
        Debug.Log(hits.Length);
        //play particle effect
        //SoundEffect
        float totalDamage = attackDamage;

        for (int i = 0; i < hits.Length; i++) 
        {

            if (isFiredUp)
            {
                totalDamage += extraDamage;
            }

            if (hits[i].transform.CompareTag("Enemy"))
            {
                Debug.Log("hit enemy pew pew");
                hits[i].transform.gameObject.GetComponentInParent<EnemyStateMachine>().ReceiveDamage(totalDamage, playerID);
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
            if (firedUpClip.sound != null) { SoundManager.instanceSM.PlaySound(firedUpClip, transform.position); }
        }
        else
        {
            if (firedDownClip.sound != null) { SoundManager.instanceSM.PlaySound(firedDownClip, transform.position); }
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
        Debug.Log("00 Start of coroutine");
        isShield = !isShield;

        if (isShield && !shieldExists)
        {
            Debug.Log("01 Spawned Shield");
            shieldReference = Instantiate(shieldPrefab, pTransform.position, quaternion.identity, transform);
            
            shieldExists = true;
        } 
        else if (!isShield && shieldExists)
        {
            Debug.Log("01 Destroyed Shield");
            Destroy(shieldReference);
            shieldMove = false;
            StopCoroutine(Shield());
        }

        if (shieldExists == true) { shieldMove = true;
            Debug.Log("02 PreventMovement");
        }

        yield return new WaitForSeconds(shieldDuration);

        Debug.Log("03 allow movement");
        shieldMove = false;
        if (shieldReference != null) { Destroy(shieldReference); shieldExists = false; Debug.Log("04 destroy shield"); }

        yield return new WaitForSeconds(shieldCooldown);
        isShield = false;
        Debug.Log("05 Shield No");
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

        if (playerHitClip.sound != null) { SoundManager.instanceSM.PlaySound(playerHitClip, transform.position); }

        if (health >= 0) 
        {
            if (deathClip.sound != null) { SoundManager.instanceSM.PlaySound(deathClip, transform.position); }
        }
    }

    public void RechargeMelee()
    {
        if (rechargeClip.sound != null) { SoundManager.instanceSM.PlaySound(rechargeClip, transform.position); }
        attackCharges++;
    }

    #endregion ------------------------    Collision    ------------------------
}
