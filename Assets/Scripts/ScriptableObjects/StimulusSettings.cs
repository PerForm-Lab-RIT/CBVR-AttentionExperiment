using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Settings/Stimulus Settings")]
    public class StimulusSettings : ScriptableObject
    {
        [Range(0, 360)]
        public float correctAngle;
        
        [Range(0, 360)]
        public float coherenceRange;
        
        [Range(0,120)]
        public float apertureRadiusDegrees;

        [Range(0,10)]
        public float density;
        
        [Range(0,100)]
        public float noiseDotPercentage;
        public float dotSizeArcMinutes;
        public float speed;
        public float minDotLifetime;
        public float maxDotLifetime;
        public float stimDepthMeters;
    }
}
