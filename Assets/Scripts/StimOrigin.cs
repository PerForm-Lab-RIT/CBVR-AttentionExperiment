using ScriptableObjects;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class StimOrigin : MonoBehaviour
{
    public void Start()
    {
        var stimulusSettings = GetComponentInChildren<StimulusSettings>();
        transform.position = new Vector3(0, 0, stimulusSettings.stimDepthMeters);
    }
}
