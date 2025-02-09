using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitching : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private string sectionInital;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.InPosition(sectionInital);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.OutPosition(sectionInital);
        }
    }
}
