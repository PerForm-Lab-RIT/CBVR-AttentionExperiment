using EyeTracker;
using ScriptableObjects;
using UnityEngine;

namespace DebugScripts
{
    public class ShowGaze : MonoBehaviour
    {

        [SerializeField] private SelectEyeTracker eyeTrackerObject;
        [SerializeField] private GameObject visual;
        [SerializeField] private Camera vrCamera;
        [SerializeField] private DebugSettings debugSettings;

        private IEyeTracker _eyeTracker;
        private Transform _cameraTransform;
        
        public void OnEnable()
        {
            _eyeTracker = eyeTrackerObject.ChosenTracker;
            _cameraTransform = vrCamera.transform;

            vrCamera.cullingMask = 1 << LayerMask.NameToLayer(debugSettings.gazeSpectatorOnly ? "Default" : "GazeSphere");
            visual.SetActive(true);
        }
    
        public void Update()
        {
            _eyeTracker.GetLocalGazeDirection();
            if (Physics.Raycast(_cameraTransform.position,
                _cameraTransform.TransformDirection(_eyeTracker.GetLocalGazeDirection()), out var hit))
            {
                visual.transform.position = hit.point;
            }
        }
    }
}
