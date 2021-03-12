using System;
using EyeTracker;
using ScriptableObjects;
using UnityEngine;

public class SelectEyeTracker : MonoBehaviour
{
    private enum ETrackerSelection
    {
        PupilLabs,
        Dummy
    }

    [SerializeField] private ETrackerSelection selection;
    [SerializeField] private GameObject pupilEyeTracker;
    [SerializeField] private bool enableDebugView;
    [SerializeField] private float debugDistance;
    [SerializeField] private Transform cameraOrigin;

    private DummyEyeTracker _dummyEyeTracker;

    public IEyeTracker ChosenTracker { get; private set; }
    
    public void Start()
    {
        _dummyEyeTracker = new DummyEyeTracker();
        switch (selection)
        {
            case ETrackerSelection.PupilLabs:
                pupilEyeTracker.SetActive(true);
                ChosenTracker = pupilEyeTracker.GetComponent<IEyeTracker>();
                break;
            case ETrackerSelection.Dummy:
                ChosenTracker = _dummyEyeTracker;
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
