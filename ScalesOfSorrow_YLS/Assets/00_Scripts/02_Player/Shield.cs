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
    [SerializeField] Sound shieldUpClip;
    [SerializeField] Sound blockClip;
    [SerializeField] Sound breakClip;

    #endregion

    void Start()
    {
        parentPlayer = transform.parent.gameObject;
        shieldCollider = GetComponent<SphereCollider>();

        if(shieldUpClip.sound != null) { SoundManager.instanceSM.PlaySound(shieldUpClip, transform.position); }
        
    }


    void Update()
    {
        Debug.Log(shieldCollider.radius);
        if (shieldHealth <= 0)
        {
            Destroy(this);
            //informs player that shield is destroyed
            parentPlayer.GetComponent<PlayerController>().ShieldDestroyed();

            if (breakClip.sound != null) { SoundManager.instanceSM.PlaySound(breakClip, transform.position); }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("DMG-Projectile"))
        {
            if (blockClip.sound != null) { SoundManager.instanceSM.PlaySound(blockClip, transform.position); }
            Destroy(collision.gameObject);
            shieldHealth--;
        }
    }
}
