using UnityEngine;

namespace EyeTracker
{
    public class DummyEyeTracker : IEyeTracker
    {
        public Vector3 GetLocalGazeDirection()
        {
            return Vector3.forward;
        }
    }
}