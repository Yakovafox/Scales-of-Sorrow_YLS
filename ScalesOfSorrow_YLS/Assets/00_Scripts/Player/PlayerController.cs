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
    private float pScale;
    #endregion
    void Start()
    {
        pTransform = transform;
        pScale = pTransform.localScale.x;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
       MoveInput();
    }

    #region Movement

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }


    void MoveInput()
    {
        Vector3 axis = new Vector3(movementInput.x, 0, movementInput.y);
        pTransform.position += (axis.normalized * pSpeed * Time.deltaTime);

        if(axis.x > 0) { pTransform.localScale = new Vector3(pScale, pTransform.localScale.y, pTransform.localScale.z); }
        else if (axis.x < 0) { pTransform.localScale = new Vector3(-pScale, pTransform.localScale.y, pTransform.localScale.z); }
    }

    #endregion
}
