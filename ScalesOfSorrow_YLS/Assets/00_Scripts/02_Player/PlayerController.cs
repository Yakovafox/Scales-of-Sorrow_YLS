using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using RotaryHeart.Lib.PhysicsExtension;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

public class PlayerController : MonoBehaviour
{
    #region ------------------------    Variables    ------------------------
    [Header("------- ID -------")]
    public int playerID;
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private Animator player1Animator;
    [SerializeField] public  RuntimeAnimatorController player2Animator;
    [Header("------- Health -------")]
    [SerializeField] private float health;

    [SerializeField] private float dmg_flashTime = 0.5f;
    [SerializeField] private AnimationCurve dmg_AnimCurve;
    [SerializeField] private Color dmg_flashColour;

    [Header("------- Movement -------")]
    [SerializeField] private float pSpeed;
    [SerializeField] private Vector2 movementInput = Vector2.zero;
    [SerializeField] private Vector3 previousInput;
    private Transform pTransform;
    private Rigidbody pRB;
    private SpriteRenderer pSR;
  
    [Header("------- Dash -------")]
    [Tooltip("dashSpeed controls how fast with the player moves during the dash. This can be used to control the distance")]
    [SerializeField] private float dashSpeed;
    private Collider pCollider;
    private bool canDash = false;
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
                     private bool canAttack = false;

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
    private bool shieldCooldownDone = true;

    [Header("------- Upgrades -------")]
    [SerializeField] private bool upgradeShield;
    public bool Acc_upgradeShield
    {
        get { return upgradeShield; }
        set { upgradeShield = value; }
    }
    [SerializeField] private bool upgradeFiredUp;
    public bool Acc_upgradeFiredUp
    {
        get { return upgradeFiredUp; }
        set { upgradeFiredUp = value; }
    }
    [SerializeField] private bool upgradeDash;
    public bool Acc_upgradeDash
    {
        get { return upgradeDash; }
        set { upgradeDash = value; }
    }

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

    private AudioSource player_audioSource;

    [Header("----- UI -----")]
    [SerializeField] private Management_GameMenus management_GameMenus;
    private GameObject canvas_Gameplay;
    [SerializeField] private GameObject tempHealth;
    private GameObject individualUI;
    private TextMeshProUGUI IDUI;
    private UnityEngine.UI.Image playerImage;
    public Sprite P2Image;
    private TextMeshProUGUI AmmoUI;
    private UnityEngine.UI.Slider ShieldUI;
    private UnityEngine.UI.Slider FiredUpUI;

    private GameObject temp;
    private UnityEngine.UI.Slider HealthBar;
    private RectTransform tempRect;

    [Header("-----Animtions-----")]
    [SerializeField] Animator animationController;

    [Header("-----Effects-----")]
    [SerializeField] ParticleSystem dashLeft;
    [SerializeField] ParticleSystem dashRight;
    [SerializeField] ParticleSystem attackEffect;
    [SerializeField] ParticleSystem attackHit;

    [Header("-----GhostMode-----")]
    [SerializeField] private bool isGhost;
    [SerializeField] private bool canBlock = true;
    [SerializeField] private float ghostCooldown;
    [SerializeField] private LayerMask excludeAttack;
    [SerializeField] private LayerMask includeAttack;
    
    public delegate void delegate_playerDefeated();
    public static event delegate_playerDefeated OnPlayerDefeated;


    #endregion ------------------------    Variables    ------------------------

    private void OnEnable()
    {
        DialogueManager.OnPlayerAttacking += enableDisableAttacking;
    }
    private void OnDisable()
    {
        DialogueManager.OnPlayerAttacking -= enableDisableAttacking;
    }

    void Start()
    {
        pTransform = transform;
        pRB = GetComponent<Rigidbody>();
        pCollider = GetComponent<Collider>();
        pSR = GetComponentInChildren<SpriteRenderer>();
        player_audioSource = GetComponentInChildren<AudioSource>();
        player1Animator = GetComponentInChildren<Animator>();

        playerDeath = FindAnyObjectByType<PlayerDeath>();
        playerDeath.AddChild(gameObject);

        attackCharges = maxCharges;

        canvas_Gameplay = GameObject.FindGameObjectWithTag("Gameplay_Canvas");
        management_GameMenus = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Management_GameMenus>();
        individualUI = Instantiate(tempHealth, canvas_Gameplay.transform);
        temp = individualUI.transform.GetChild(1).gameObject;

        IDUI = individualUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        IDUI.text = "P" + (playerID + 1);
        playerImage = IDUI.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        if (playerID + 1 == 2)
        {
            playerImage.sprite = P2Image;
        }

        HealthBar = individualUI.transform.GetChild(2).GetComponent<UnityEngine.UI.Slider>();
        HealthBar.value = ValueConverter0to1(health, 0, 100);
        AmmoUI = individualUI.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        AmmoUI.text = attackCharges.ToString();

        ShieldUI = individualUI.transform.GetChild(4).GetComponent<UnityEngine.UI.Slider>();
        if (!upgradeShield) { ShieldUI.gameObject.SetActive(false); }

        FiredUpUI = individualUI.transform.GetChild(5).GetComponent<UnityEngine.UI.Slider>();
        if (!upgradeFiredUp) { FiredUpUI.gameObject.SetActive(false); }

        tempRect = individualUI.GetComponent<RectTransform>();
        if (playerID == 0)
        {
            tempRect.anchoredPosition = new Vector2(-280, -150);
        }
        else
        {
            tempRect.anchoredPosition = new Vector2(320, -150);
            player1Animator.runtimeAnimatorController = player2Animator as RuntimeAnimatorController;
        }

        transform.position = new Vector3(8, transform.position.y, 4);
        canDash = true;
    }

    void Update()
    {
        if (isDash) { return; }
    }

    private void FixedUpdate()
    {
        if (isDash) { return; }
        if (!isShield) { MoveInput(); }
    }
    public void SetPlayerID(int ID)
    { 
        playerID = ID;
    }

    #region ------------------------    Movement    ------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

        if (movementClip.sound != null) { SoundManager.instanceSM.PlaySound(movementClip, transform.position); }

    }
    
    private void MoveInput()
    {
        Vector3 axis = new Vector3(movementInput.x, 0, movementInput.y);
        if (movementInput != Vector2.zero) { previousInput = new Vector3(movementInput.x, 0, movementInput.y); }
        pRB.velocity = (axis.normalized * (pSpeed * Time.deltaTime));

        if(axis.x > 0) { transform.localScale = new Vector3(.25f, .25f, .25f); }
        else if (axis.x < 0) { transform.localScale = new Vector3(-.25f, .25f, .25f); }

        if (axis.x != 0 | axis.z != 0) {animationController.SetBool("isRunning", true); }
        else { animationController.SetBool("isRunning", false); }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
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
            Luke_SoundManager.PlaySound(SoundType.PlayerDash, 1, player_audioSource);
        }
    }

    private IEnumerator DefaultDash()
    {
        pCollider.excludeLayers = excludeLayers;
        canDash = false;
        isDash = true;

        animationController.SetTrigger("hasDashed");

        if (previousInput.x > 0 && (!dashLeft.isPlaying)) { dashLeft.Play(); }
        if (previousInput.x < 0 && (!dashRight.isPlaying)) { dashRight.Play(); }

        pRB.velocity = (previousInput) * dashSpeed;

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
        AmmoUI.text = attackCharges.ToString();

        animationController.SetTrigger("hasAttacked");

        if (!attackEffect.isPlaying) { attackEffect.Play(); }


        Vector3 direction = previousInput;

        RaycastHit[] hits = Physics.SphereCastAll(pTransform.position, attackSize, direction, attackRange, attackMask/*, PreviewCondition.Both, 1f, Color.green, Color.red*/);
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
                if (!attackHit.isPlaying) { attackHit.Play(); }
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void enableDisableAttacking()
    {
        canAttack = !canAttack;
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
        if (upgradeShield && context.started && shieldCooldownDone)
        {
            StartCoroutine(ShieldUp());
        }
    }

    IEnumerator ShieldUp()
    {
        shieldCooldownDone = false;
        Debug.Log("00 Start of coroutine");
        isShield = true;
        ShieldUI.value = ValueFlipper(0);
        shieldReference = Instantiate(shieldPrefab, pTransform.position, quaternion.identity, transform);
        
        yield return new WaitForSeconds(shieldDuration);

        Debug.Log("03 allow movement");
        isShield = false;
        if (shieldReference != null) { Destroy(shieldReference); }

        yield return new WaitForSeconds(shieldCooldown);
        ShieldUI.value = ValueFlipper(1);
        shieldCooldownDone = true;
        Debug.Log("05 Shield No");
    }
    public void ShieldDestroyed()
    {
        isShield = false;
    }
    #endregion ------------------------    Shield    ------------------------

    #region ------------------------    Collision    ------------------------
    public void TakeDamage(float damage)
    {
        if (isShield) { return; }
        else if (isGhost & canBlock) {  StartCoroutine(GhostHit()); }
        else 
        {
            StartCoroutine(playerDamageFlash());

            health -= damage;
            HealthBar.value = ValueConverter0to1(health, 0, 100);
            
            if (playerHitClip.sound != null) { SoundManager.instanceSM.PlaySound(playerHitClip, transform.position); }

            if (health <= 0)
            {
                if (deathClip.sound != null) { SoundManager.instanceSM.PlaySound(deathClip, transform.position); }
                
                temp.SetActive(false);
                
                playerDeath.RemoveChild(gameObject);

                pSR.enabled = false;

                EnterGhost();
            }
        }

    }
    private IEnumerator playerDamageFlash()
    {
        float currentFlashValue = 0f;
        float elapsedTime = 0f;
        SpriteRenderer spriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();

        while (elapsedTime < dmg_flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashValue = Mathf.Lerp(1f, dmg_AnimCurve.Evaluate(elapsedTime), (elapsedTime / dmg_flashTime));
            spriteRenderer.material.SetColor("_FlashColour", dmg_flashColour);
            spriteRenderer.material.SetFloat("_FlashAmount", currentFlashValue);

            yield return new WaitForSeconds(0.01f);
        }
    }

    public void RechargeMelee()
    {
        if (rechargeClip.sound != null) { SoundManager.instanceSM.PlaySound(rechargeClip, transform.position); }
        attackCharges++;
        AmmoUI.text = attackCharges.ToString();
    }

    #endregion ------------------------    Collision    ------------------------

    #region ------------------------    Ghost Mode    ------------------------
    
    private void EnterGhost()
    {
        isGhost = true;
        canAttack = false;
    }

    private IEnumerator GhostHit()
    {
        canBlock = false;
        pCollider.excludeLayers = excludeAttack;
        yield return new WaitForSeconds(ghostCooldown);
        canBlock = true;
        pCollider.excludeLayers = includeAttack;
        yield return null;
    }

    #endregion ------------------------    Ghost Mode    ------------------------

    private float ValueConverter0to1(float value, float minValue, float maxValue)
    {
        Debug.Log(value);
        Debug.Log((value - minValue) / (maxValue - minValue));
        return (value - minValue) / (maxValue - minValue);
    }

    private float ValueFlipper(float valueToFlip)
    {
        return 1 - valueToFlip;
    }

    public void upgrade()
    {
        if (upgradeShield) { ShieldUI.gameObject.SetActive(true); }
        if (upgradeFiredUp) { FiredUpUI.gameObject.SetActive(true); }
        if (upgradeDash) { return; }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            print("has paussssssssssssssssssed");
            management_GameMenus.pauseGame();
            //Pause Functionality
        }
    }

}
