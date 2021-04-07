using EyeTracker;
using UnityEngine;

namespace DebugScripts
{
    public class ShowGaze : MonoBehaviour
    {

        [SerializeField] private SelectEyeTracker eyeTrackerObject;
        [SerializeField] private GameObject visual;
        [SerializeField] private Transform cameraTransform;

        private IEyeTracker _eyeTracker;
        
        public void OnEnable()
        {
            _eyeTracker = eyeTrackerObject.ChosenTracker;
            visual.SetActive(true);
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
}
