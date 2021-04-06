using System;
using EyeTracker;
using ScriptableObjects;
using UnityEngine;
using UXF;

public class SelectEyeTracker : MonoBehaviour
{
    private enum ETrackerSelection
    {
        PupilLabs,
        ViveProEye,
        Dummy
    }

    [SerializeField] private ETrackerSelection selection;
    [SerializeField] private GameObject pupilEyeTracker;
    [SerializeField] private GameObject viveProEye;
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
            case ETrackerSelection.ViveProEye:
                viveProEye.SetActive(true);
                ChosenTracker = viveProEye.GetComponent<IEyeTracker>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SelectFromUxf()
    {
        var tracker = (string) Session.instance.participantDetails["Eyetracker"];
        switch (tracker)
        {
            case "Dummy":
                ChosenTracker = _dummyEyeTracker;
                selection = ETrackerSelection.Dummy;
                break;
            case "Pupil Labs":
                pupilEyeTracker.SetActive(true);
                ChosenTracker = pupilEyeTracker.GetComponent<IEyeTracker>();
                selection = ETrackerSelection.PupilLabs;
                break;
            case "VIVE Pro Eye":
                viveProEye.SetActive(true);
                ChosenTracker = viveProEye.GetComponent<IEyeTracker>();
                selection = ETrackerSelection.ViveProEye;
                break;
        }
    }

    public void Update()
    {
        if (enableDebugView)
            Debug.DrawRay(cameraOrigin.position, debugDistance * cameraOrigin.TransformDirection(ChosenTracker.GetLocalGazeDirection()));
    }
}
