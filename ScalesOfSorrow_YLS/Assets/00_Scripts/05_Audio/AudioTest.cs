using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{

    [SerializeField] private AudioClip testClip;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private bool loops;

    [SerializeField] private float replayDuration;
    private float loopCounter;

    void Update()
    {
        loopCounter += Time.deltaTime;
        if (loopCounter > replayDuration)
        {
            if (testClip != null) { SoundManager.instanceSM.PlaySound(testClip, cameraPosition.position, loops); }
        }
    }
}
