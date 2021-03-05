using ScriptableObjects;
using UnityEngine;

public class StimulusSpacer : MonoBehaviour
{
    [SerializeField] private StimulusSettings innerStimulusSettings;
    [SerializeField] private SessionSettings sessionSettings;

    public void OnEnable()
    {
        var innerStimulusRadius = Mathf.Tan(innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180f)
                                  * sessionSettings.stimulusDepth;
        GetComponent<MeshRenderer>().material.color = sessionSettings.skyColor;
        transform.localPosition = 
            new Vector3(0, -sessionSettings.stimulusSpacing / 2, 0);
        gameObject.transform.localScale = new Vector3(innerStimulusRadius * 2, innerStimulusRadius * 2, 0);
    }
}
