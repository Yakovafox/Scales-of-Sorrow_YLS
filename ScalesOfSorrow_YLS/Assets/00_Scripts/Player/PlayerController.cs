using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Varriables
    [Header("Lives")]
    [SerializeField] private int pLives;
    //UI Variables
    [Header("Movement")]
    [SerializeField] private Transform pTransform;
    [SerializeField] private float pSpeed;
    #endregion
    void Start()
    {
        pTransform = transform;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
       MoveInput();
    }

    #region Movement
    void MoveInput()
    {
        float zAxis = Input.GetAxisRaw("Vertical") * pSpeed * Time.deltaTime;
        float xAxis = Input.GetAxisRaw("Horizontal") * pSpeed * Time.deltaTime;
        Vector3 axis = new Vector3(xAxis, 0, zAxis);
        pTransform.position += axis;

        if(xAxis > 0) { FlipSprite(true); }
        else if (xAxis < 0) { FlipSprite(false); }
    }
    void FlipSprite( bool left)
    {
        if (left) { pTransform.localScale = new Vector3(.25f, pTransform.localScale.y, pTransform.localScale.z); }
        else if (!left) { pTransform.localScale = new Vector3(-.25f, pTransform.localScale.y, pTransform.localScale.z); }
    }

    #endregion
}
