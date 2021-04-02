using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class AdjustClippingPlanes : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private SessionSettings settings;
    // Start is called before the first frame update
    public void Start()
    {
        camera.farClipPlane = settings.stimulusDepth + 1f;
    }
}
