using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region Variables
    [Header("Shield")]
    [Tooltip("Controls Shield Health")]
    [SerializeField] private int shieldHealth;
    private GameObject parentPlayer;
    private SphereCollider shieldCollider;

    #endregion

    void Start()
    {
        parentPlayer = transform.parent.gameObject;
        shieldCollider = GetComponent<SphereCollider>();
    }


    void Update()
    {
        Debug.Log(shieldCollider.radius);
        if (shieldHealth <= 0)
        {
            Destroy(this);
            //informs player that shield is destroyed
            parentPlayer.GetComponent<PlayerController>().ShieldDestroyed();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("DMG-Projectile"))
        {
            Destroy(collision.gameObject);
            shieldHealth--;
        }
    }
}
