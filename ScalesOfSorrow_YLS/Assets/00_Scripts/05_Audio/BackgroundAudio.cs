using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAudio : MonoBehaviour
{

    [SerializeField] private Sound testClip;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private bool loops;

    [SerializeField] private float replayDuration;
    private float loopCounter;
    private void Start()
    {
        loopCounter = replayDuration;
    }
    void Update()
    {
        loopCounter += Time.deltaTime;
        if (loopCounter > replayDuration && !loops)
        {
            if (testClip != null) { SoundManager.instanceSM.PlaySound(testClip, cameraPosition.position);
                loopCounter = 0;
            }
        }

        else { SoundManager.instanceSM.PlaySound(testClip, cameraPosition.position); }
    }
}
