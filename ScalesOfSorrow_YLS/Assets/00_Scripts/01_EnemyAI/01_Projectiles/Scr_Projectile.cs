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

    [SerializeField] private bool ProjectileType_isAmmo;

    [SerializeField] private float launchForce = 25f;
    public float Acc_launchForce
    {
        set { launchForce = value; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 aimedLocation = playerDir; // Quaternion.Euler(0, 15, 0) * transform.forward;
        rb.velocity = aimedLocation * launchForce;
    }


    private void OnCollisionEnter(Collision collision)
    {
        switch (ProjectileType_isAmmo)
        {
            case false:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<PlayerController>().TakeDamage(damageToDeal);
                }

                if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enm_Shield")) return;
                break;

            case true:
                if (collision.gameObject.CompareTag("Player"))
                {
                    //Give player ammo.
                    collision.gameObject.GetComponent<PlayerController>().RechargeMelee();
                }
                if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enm_Shield")) return;
                break;
    }
        
        Destroy(gameObject);
    }
}
