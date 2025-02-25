using System.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


public class CameraShaker : MonoBehaviour
{
    [Tooltip("Set the Default time that the screen shakes for. (Called by events)")]
    [SerializeField]
    private float defaultShake_Time = 0.25f;

    [Tooltip("Set the Default strength for the amount that the screen shakes. (Called by events)")]
    [SerializeField]
    private float defaultShake_Strength = 0.5f;

    //Event Triggers go below here in OnEnable and OnDisable.
    //Shake from event will be a small shake burst that is a default, The cameraShake function can be accessed publicly to create your own level of camera shake.

    private void OnEnable()
    {
        EnemyStateMachine.OnDragonLanded += cameraShakeFromEvent;
    }

    private void OnDisable()
    {
        EnemyStateMachine.OnDragonLanded -= cameraShakeFromEvent;
    }
    
    
    public void cameraShake(float duration, float strength) //Personalisable camera shake accessible from other scripts.
    {
        StartCoroutine(Shake(duration, strength));
    }

    void cameraShakeFromEvent() //Default cameraShake, called from event.
    {
        StartCoroutine(Shake(defaultShake_Time, defaultShake_Strength));
    }

    IEnumerator Shake(float duration, float strength) //Coroutine for actually shaking the camera, picks random point within a unit of the current position and moves there over time.
    {
        Vector3 originalPos = transform.localPosition;
        
        float shakeTimer = 0.0f;

        while (shakeTimer < duration)
        {
            float movementValue = 1.0f;
            float x = Random.Range(-movementValue, movementValue) * strength;
            float y = Random.Range(-movementValue, movementValue) * strength;
            
            transform.localPosition = new Vector3(x + originalPos.x, y + originalPos.y, originalPos.z);
            
            shakeTimer += Time.deltaTime;
            
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
