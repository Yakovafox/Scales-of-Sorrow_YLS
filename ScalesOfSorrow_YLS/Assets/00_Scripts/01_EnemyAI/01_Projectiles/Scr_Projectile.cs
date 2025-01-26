using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Projectile : MonoBehaviour
{

    private Rigidbody rb;
    private Vector3 playerDir;
    public Vector3 Accessor_dir
    {
        set { playerDir = value; }
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

        Destroy(gameObject);
    }
}
