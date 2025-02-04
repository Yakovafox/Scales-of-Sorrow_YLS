using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region Variables
    [Header("Shield")]
    [Tooltip("Controls Shield Health")]
    [SerializeField] private int shieldHealth;
    private GameObject parentPlayer;

    #endregion

    void Start()
    {
        parentPlayer = transform.parent.gameObject;
    }


    void Update()
    {
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
