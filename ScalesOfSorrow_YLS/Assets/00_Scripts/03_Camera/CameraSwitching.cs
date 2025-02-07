using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitching : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private CinemachineFreeLook centralCamera;
    [SerializeField] private CinemachineFreeLook switchCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.SwitchCamera(switchCamera);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.SwitchCamera(centralCamera);
        }
    }
}
