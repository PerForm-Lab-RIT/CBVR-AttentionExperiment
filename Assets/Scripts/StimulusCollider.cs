using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class StimulusCollider : MonoBehaviour
{
    [SerializeField] private StimulusSettings settings;
    [SerializeField] private BoxCollider boxCollider;
    
    public void OnEnable()
    {
        var apertureRadius = Mathf.Tan(settings.apertureRadiusDegrees * Mathf.PI / 180) *
            settings.stimDepthMeters;
        boxCollider.size = new Vector3(apertureRadius * 2f, 0.005f, apertureRadius * 2f);
        transform.localPosition = new Vector3(0.0f, 0.0f, settings.stimDepthMeters);
    }
}
