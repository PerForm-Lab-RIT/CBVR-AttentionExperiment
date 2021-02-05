using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class AdjustSelectionSize : MonoBehaviour
{
    [SerializeField] private StimulusSettings settings;
    
    public void OnEnable()
    {
        var newScale = 2 * Mathf.Tan(settings.apertureRadiusDegrees * Mathf.PI / 180) *
                       settings.stimDepthMeters;
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }
}
