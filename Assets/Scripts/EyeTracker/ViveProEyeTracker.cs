using Tobii.XR;
using UnityEngine;

namespace EyeTracker
{
    public class ViveProEyeTracker : MonoBehaviour, IEyeTracker
    {
        public Vector3 GetLocalGazeDirection()
        {
            var trackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);
            return trackingData.GazeRay.Direction;
        }
    }
}
