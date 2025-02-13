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
    [Space]
    [SerializeField] AudioClip shieldUpClip;
    [SerializeField] AudioClip blockClip;
    [SerializeField] AudioClip breakClip;

    #endregion

    void Start()
    {
        parentPlayer = transform.parent.gameObject;
        shieldCollider = GetComponent<SphereCollider>();

        if(shieldUpClip != null) { SoundManager.instanceSM.PlaySound(shieldUpClip, transform.position, false); }
        
    }


    void Update()
    {
        Debug.Log(shieldCollider.radius);
        if (shieldHealth <= 0)
        {
            Destroy(this);
            //informs player that shield is destroyed
            parentPlayer.GetComponent<PlayerController>().ShieldDestroyed();

            if (breakClip != null) { SoundManager.instanceSM.PlaySound(breakClip, transform.position, false); }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("DMG-Projectile"))
        {
            if (blockClip != null) { SoundManager.instanceSM.PlaySound(blockClip, transform.position, false); }
            Destroy(collision.gameObject);
            shieldHealth--;
        }
    }
}
