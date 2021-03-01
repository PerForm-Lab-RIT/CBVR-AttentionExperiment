using UnityEngine;

namespace EyeTracker
{
    public interface IEyeTracker
    {
        Vector3 GetLocalGazeDirection();
    }
}