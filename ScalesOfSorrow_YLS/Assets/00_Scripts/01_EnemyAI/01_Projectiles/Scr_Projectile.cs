using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Projectile : MonoBehaviour
{

    private Rigidbody rb;

    [SerializeField] private float launchForce = 25f;

    void Start()
    {
        Vector3 aimedLocation = transform.rotation * Quaternion.Euler(0, 15, 0) * transform.forward;
        rb.velocity = aimedLocation * launchForce;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            //Deal Damage to player;
        }

        Destroy(gameObject);
    }
}
