using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Trap : MonoBehaviour
{
    private GameObject detectedPlayer;

    private GameObject particleEffect_ToPlay;

    [SerializeField]
    private float bufferTime = 0.5f;

    [SerializeField]
    private float damageToDeal;

    void Awake()
    {
        particleEffect_ToPlay = transform.GetChild(0).gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            detectedPlayer = other.gameObject;
            StartCoroutine(safetyBuffer());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == detectedPlayer)
        {
            detectedPlayer = null;
        }
    }

    IEnumerator safetyBuffer()
    {
        yield return new WaitForSeconds(bufferTime);
        if (detectedPlayer == null) { yield return null; }
            detectedPlayer.GetComponent<PlayerController>().TakeDamage(damageToDeal);

        particleEffect_ToPlay.GetComponent<ParticleSystem>().Play();
    }
}
