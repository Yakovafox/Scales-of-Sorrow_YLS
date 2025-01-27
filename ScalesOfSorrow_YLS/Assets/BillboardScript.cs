using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardScript : MonoBehaviour
{
    [SerializeField] private BillboardMode currentMode;

    [Header("RotationLocks")]
    [SerializeField] private bool xLock;
    [SerializeField] private bool yLock;
    [SerializeField] private bool zLock;

    private Vector3 originalRotation;

    public enum BillboardMode
    {
        LookAtCam, CamForward
    };

    private void Awake()
    {
        originalRotation = transform.rotation.eulerAngles;
    }

    private void LateUpdate()
    {
        switch (currentMode)
        {
            case BillboardMode.LookAtCam:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;

            case BillboardMode.CamForward:
                transform.forward = Camera.main.transform.forward;
                break;

            default:
                break;
        }

        Vector3 modifyRotation = transform.rotation.eulerAngles;
        if (xLock) { modifyRotation.x = originalRotation.x; }
        if (yLock) { modifyRotation.y = originalRotation.y; }
        if (zLock) { modifyRotation.z = originalRotation.z; }

        transform.rotation = Quaternion.Euler(modifyRotation);
    }

}
