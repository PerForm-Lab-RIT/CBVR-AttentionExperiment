using ScriptableObjects;
using UnityEngine;

public class StimulusSpacer : MonoBehaviour
{
    [SerializeField] private StimulusSettings innerStimulusSettings;
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private Transform innerStimulusTransform;

    private Transform prevInnerTransform;

    public void OnEnable()
    {
        var spacerRadius = Mathf.Tan(innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180f)
                                  * sessionSettings.stimulusDepth;
        GetComponent<MeshRenderer>().material.color = sessionSettings.skyColor;
        gameObject.transform.localScale = new Vector3(spacerRadius * 2, spacerRadius * 2, 0);
        prevInnerTransform = innerStimulusTransform;
    }

    public void Update()
    {
        var localPosition =innerStimulusTransform.localPosition;
        gameObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y,
            localPosition.z + sessionSettings.stimulusSpacing / 2);
    }
}
