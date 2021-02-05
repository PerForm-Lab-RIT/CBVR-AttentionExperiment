using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ActiveLaserManager : MonoBehaviour
{
    [SerializeField] private GameObject leftHandPointer;
    [SerializeField] private GameObject rightHandPointer;
    
    public GameObject ActiveLaser { get; private set; }
    public SteamVR_Action_Vector2 angleSelectAction;
    public SteamVR_Action_Boolean confirmAction;

    private bool _deactivated;
    
    public void Start()
    {
        ActiveLaser = rightHandPointer;
        confirmAction[SteamVR_Input_Sources.LeftHand].onChange += UpdateActiveLaser;
        confirmAction[SteamVR_Input_Sources.RightHand].onChange += UpdateActiveLaser;
        angleSelectAction[SteamVR_Input_Sources.LeftHand].onChange += UpdateActiveLaser;
        angleSelectAction[SteamVR_Input_Sources.RightHand].onChange += UpdateActiveLaser;
    }

    private void UpdateActiveLaser(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool state)
    {
        GameObject inactiveLaser;
        if (source == SteamVR_Input_Sources.RightHand)
        {
            ActiveLaser = rightHandPointer;
            inactiveLaser = leftHandPointer;
        }
        else
        {
            ActiveLaser = leftHandPointer;
            inactiveLaser = rightHandPointer;
        }

        if (!_deactivated)
        {
            ActiveLaser.SetActive(true);
            inactiveLaser.SetActive(false);
        }
    }
    
    private void UpdateActiveLaser(SteamVR_Action_Vector2 action, SteamVR_Input_Sources source, Vector2 axis, Vector2 delta)
    {
        GameObject inactiveLaser;
        if (source == SteamVR_Input_Sources.RightHand)
        {
            ActiveLaser = rightHandPointer;
            inactiveLaser = leftHandPointer;
        }
        else
        {
            ActiveLaser = leftHandPointer;
            inactiveLaser = rightHandPointer;
        }

        if (!_deactivated)
        {
            ActiveLaser.SetActive(true);
            inactiveLaser.SetActive(false);
        }
    }

    public void DeactivateBothLasers()
    {
        leftHandPointer.SetActive(false);
        rightHandPointer.SetActive(false);
        _deactivated = true;
    }

    public void ActivateLaser()
    {
        ActiveLaser.SetActive(true);
        _deactivated = false;
    }
}
