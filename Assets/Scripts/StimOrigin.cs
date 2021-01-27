using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using ScriptableObjects;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class StimOrigin : MonoBehaviour
{
    [SerializeField] private StimulusSettings stimulusSettings;
    
    
    public void Start()
    {
        transform.position = new Vector3(0, 0, stimulusSettings.stimDepthMeters);
    }
}
