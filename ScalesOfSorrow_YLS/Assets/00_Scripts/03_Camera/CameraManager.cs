using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] cameras;

    [SerializeField] private CinemachineVirtualCamera startCamera, currentCamera;

    [SerializeField] private bool inCentral, inLeft, inRight;
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
    private void FixedUpdate() { SwitchCamera(); }

    public void SwitchCamera()
    {
        currentCamera.Priority = 0;

        if (inCentral) { ChooseCamera(0); }
        else if (inLeft && inRight) { ChooseCamera(0); }
        else if (inLeft && !inRight && !inCentral) { ChooseCamera(1); }
        else if (!inLeft && inRight && !inCentral) { ChooseCamera(2); }

    }

    private void ChooseCamera(int index)
    {
        currentCamera = cameras[index];
        currentCamera.Priority = 20;
    }

    public void InPosition(string section)
    {
        if (section == "C") { inCentral = true; }
        else if (section == "L") { inLeft = true; }
        else if (section == "R") { inRight = true; }
    }
    public void OutPosition(string section)
    {
        if (section == "C") { inCentral = false; }
        else if (section == "L") { inLeft = false; }
        else if (section == "R") { inRight = false; }
    }
}
