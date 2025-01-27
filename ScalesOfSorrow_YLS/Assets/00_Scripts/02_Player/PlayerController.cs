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
    [SerializeField] private int pLives;
    //UI Variables
    [Header("Movement")]
    [SerializeField] private float pSpeed;
    [SerializeField] private Vector2 movementInput = Vector2.zero;
    private Transform pTransform;
    private Rigidbody pRB;
    [SerializeField] private SpriteRenderer pSR;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] private bool canDash;
    [SerializeField] private bool isDash;

    [Header("Attack")]
    [SerializeField] private LayerMask attackMask;
    [SerializeField] private bool canAttack;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackSize;
    [SerializeField] private float aimSpeed;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackDamage;
    [SerializeField] private int attackCharges;
    [SerializeField] private int maxCharges;

    [Header("Upgrades")]
    [SerializeField] private bool upgradeShield;
    [SerializeField] private bool upgradeSuper;

    #endregion
    void Start()
    {
        pTransform = transform;
        pRB = GetComponent<Rigidbody>();

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
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash) { StartCoroutine(DefaultDash()); }
    }

    private IEnumerator DefaultDash()
    {
        canDash = false;
        isDash = true;

        pRB.velocity = new Vector3(movementInput.x, 0, movementInput.y) * dashSpeed;

        yield return new WaitForSeconds(dashTime);
        isDash = false;
        pRB.velocity = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    #endregion

    #region Attack --------------------------------------------------------------------------------------------------------------------------

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack) { return; }
        else if (attackCharges <= 0) { return; }
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

}
