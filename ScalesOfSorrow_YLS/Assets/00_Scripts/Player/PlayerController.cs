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
        
    }


    void MoveInput()
    {
        float zAxis = Input.GetAxisRaw("Vertical") * pSpeed * Time.deltaTime;
        float xAxis = Input.GetAxisRaw("Horizontal") * pSpeed * Time.deltaTime;
        Vector3 axis = new Vector3(xAxis, 0, zAxis);
        pTransform.position += axis;

        if(xAxis > 0) { pTransform.localScale = new Vector3(pScale, pTransform.localScale.y, pTransform.localScale.z); }
        else if (xAxis < 0) { pTransform.localScale = new Vector3(-pScale, pTransform.localScale.y, pTransform.localScale.z); }
    }

    #endregion
}
