using EyeTracker;
using UnityEngine;

public class ShowGaze : MonoBehaviour
{
    [SerializeField] private SelectEyeTracker eyeTrackerObject;
    [SerializeField] private GameObject visual;
    [SerializeField] private Transform cameraTransform;

    private IEyeTracker _eyeTracker;
    
    public void OnEnable()
    {
        _eyeTracker = eyeTrackerObject.ChosenTracker;
    }
    
    public void Update()
    {
        _eyeTracker.GetLocalGazeDirection();
        if (Physics.Raycast(cameraTransform.position,
            cameraTransform.TransformDirection(_eyeTracker.GetLocalGazeDirection()), out var hit))
        {
            visual.transform.position = hit.point;
        }
    }
}
