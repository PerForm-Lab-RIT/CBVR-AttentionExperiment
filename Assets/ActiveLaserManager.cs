using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ActiveLaserManager : MonoBehaviour
{
    [SerializeField] private GameObject leftHandPointer;
    [SerializeField] private GameObject rightHandPointer;
    
    public SteamVR_Action_Vector2 angleSelectAction;
    public SteamVR_Action_Boolean confirmAction;

    private GameObject _activeLaser;
    private bool _deactivated;
    
    public void Start()
    {
        _activeLaser = rightHandPointer;
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
            _activeLaser = rightHandPointer;
            inactiveLaser = leftHandPointer;
        }
        else
        {
            _activeLaser = leftHandPointer;
            inactiveLaser = rightHandPointer;
        }

        if (!_deactivated)
        {
            _activeLaser.SetActive(true);
            inactiveLaser.SetActive(false);
        }
    }
    
    private void UpdateActiveLaser(SteamVR_Action_Vector2 action, SteamVR_Input_Sources source, Vector2 axis, Vector2 delta)
    {
        GameObject inactiveLaser;
        if (source == SteamVR_Input_Sources.RightHand)
        {
            _activeLaser = rightHandPointer;
            inactiveLaser = leftHandPointer;
        }
        else
        {
            _activeLaser = leftHandPointer;
            inactiveLaser = rightHandPointer;
        }

        if (!_deactivated)
        {
            _activeLaser.SetActive(true);
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
        _activeLaser.SetActive(true);
        _deactivated = false;
    }

    public Transform GetActiveSelectionTransform()
    {
        // LaserPointer prefab should have the selection circle as its only immediate child
        var selectionTransform = _activeLaser.gameObject.transform.GetChild(0);
        return selectionTransform;
    }
}
