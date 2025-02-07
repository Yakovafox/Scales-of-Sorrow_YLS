using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] public CinemachineFreeLook[] cameras;

    private CinemachineFreeLook startCamera, currentCamera;
    void Start()
    {
        startCamera = cameras[0];
        currentCamera = startCamera;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == currentCamera)
            {
                cameras[i].Priority = 20;
            }
            else
            {
                cameras[i].Priority = 0;
            }
        }
    }

    public void SwitchCamera(CinemachineFreeLook newCamera) 
    {
        currentCamera.Priority = 0;
        currentCamera = newCamera;
        currentCamera.Priority = 20;
    }
}
