using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Projectile : MonoBehaviour
{

    private Rigidbody rb;
    private Vector3 playerDir;
    [SerializeField] private float damageToDeal;
    public Vector3 Accessor_dir
    {
        set { playerDir = value; }
    }

    public float Accessor_damageToDeal
    {
        set { damageToDeal = value; }
    }

    [SerializeField] private float launchForce = 25f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 aimedLocation = playerDir; // Quaternion.Euler(0, 15, 0) * transform.forward;
        rb.velocity = aimedLocation * launchForce;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            //Deal Damage to player;
        }

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enm_Shield")) return;
        
        Destroy(gameObject);
    }
}
