using ScriptableObjects;
using UnityEngine;

public class FixationCheckColorChange : MonoBehaviour
{
    [SerializeField] private SessionSettings settings;
    [SerializeField] private MeshRenderer dotMesh;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private SelectEyeTracker eyeTrackerSelect;

    private float _errorRadius;

    public void OnEnable()
    {
        dotMesh.material.color = Color.white;
        _errorRadius = Mathf.Tan(settings.fixationErrorTolerance * Mathf.PI / 180 * settings.stimulusDepth);
    }
    
    public void Update()
    {
        if (Physics.Raycast(cameraTransform.position,
            cameraTransform.TransformDirection(eyeTrackerSelect.ChosenTracker.GetLocalGazeDirection()), out var hit))
            if ((hit.point - gameObject.transform.position).magnitude <= _errorRadius)
                dotMesh.material.color = Color.yellow;
            else
                dotMesh.material.color = Color.white;
    }
}
