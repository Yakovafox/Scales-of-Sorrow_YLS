using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private float pScale;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    private bool canDash;
    private bool isDash;

    #endregion
    void Start()
    {
        pTransform = transform;
        pRB = GetComponent<Rigidbody>();
        pScale = pTransform.localScale.x;
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

    #region Movement

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
    public void OnDash(InputAction.CallbackContext context) 
    {
        StartCoroutine(Dash());
    }


    void MoveInput()
    {
        Vector3 axis = new Vector3(movementInput.x, 0, movementInput.y);
        pTransform.position += (axis.normalized * pSpeed * Time.deltaTime);


        if(axis.x > 0) { pTransform.localScale = new Vector3(pScale, pTransform.localScale.y, pTransform.localScale.z); }
        else if (axis.x < 0) { pTransform.localScale = new Vector3(-pScale, pTransform.localScale.y, pTransform.localScale.z); }
    }

    private IEnumerator Dash()
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
}
