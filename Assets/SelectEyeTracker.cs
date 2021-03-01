using System;
using EyeTracker;
using UnityEngine;

public class SelectEyeTracker : MonoBehaviour
{
    private enum ETrackerSelection
    {
        PupilLabs
    }

    [SerializeField] private ETrackerSelection selection;
    [SerializeField] private GameObject pupilEyeTracker;
    [SerializeField] private bool enableDebugView;
    [SerializeField] private float debugDistance;
    [SerializeField] private Transform cameraOrigin;

    public IEyeTracker ChosenTracker { get; private set; }
    
    public void Start()
    {
        switch (selection)
        {
            case ETrackerSelection.PupilLabs:
                pupilEyeTracker.SetActive(true);
                ChosenTracker = pupilEyeTracker.GetComponent<IEyeTracker>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Update()
    {
        if (enableDebugView)
            Debug.DrawRay(cameraOrigin.position, debugDistance * cameraOrigin.TransformDirection(ChosenTracker.GetLocalGazeDirection()));
    }
}
