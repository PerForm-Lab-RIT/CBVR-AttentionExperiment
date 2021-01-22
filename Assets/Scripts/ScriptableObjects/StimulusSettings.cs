using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Settings/Stimulus Settings")]
    public class StimulusSettings : ScriptableObject
    {
        public float duration;
        public List<float> staircaseAngles;
        public float xOffset;
        public float yOffset;
        
        [Range(0,120)]
        public float apertureRadiusDegrees;
        public float trialBridgeInterval;
        
        [Range(0,10)]
        public float density;
        
        [Range(0,100)]
        public float noiseDotPercentage;
        public float dotSizeArcMinutes;
        public float speed;
        public float dotLifetime;
        public float stimDepthMeters;
        
    }
}
