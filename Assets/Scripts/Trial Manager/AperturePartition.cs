using ScriptableObjects;
using UnityEngine;

namespace Trial_Manager
{
    public class AperturePartition
    {
        private readonly SessionSettings _sessionSettings;

        private readonly float _sliceSize;

        private readonly float _outerApertureRadius;
        private readonly float _innerApertureRadius;

        public (float, float)[] ApertureSlices { get; }
        public string[] SliceLabels { get; }

        public AperturePartition(SessionSettings sessionSettings, 
            StimulusSettings outerStimulusSettings, StimulusSettings innerStimulusSettings)
        {
            _sessionSettings = sessionSettings;

            _outerApertureRadius = Mathf.Tan(outerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                                   sessionSettings.stimulusDepth;
            _innerApertureRadius = Mathf.Tan(innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                                   sessionSettings.stimulusDepth;
        
            if(_sessionSettings.sessionType == SessionSettings.SessionType.Training)
            {
            ApertureSlices = new (float, float)[_sessionSettings.regionSlices / 2];
            SliceLabels = new string[_sessionSettings.regionSlices / 2];
            }
            else
            {
                ApertureSlices = new (float, float)[_sessionSettings.regionSlices];
                SliceLabels = new string[_sessionSettings.regionSlices];
            }

            _sliceSize = 360.0f / _sessionSettings.regionSlices;
            PartitionAperture();
        }
    
        private void PartitionAperture()
        {
            if(_sessionSettings.regionSlices % 2 != 0)
                Debug.LogWarning("Odd number of aperture slices detected! Please use an even number.");
        
            int startRegion;
            if (_sessionSettings.sessionType == SessionSettings.SessionType.Training)
            {
                startRegion = (_sessionSettings.flipRegions) ? 1 : 0;
                var k = 0;
                for (var i = startRegion; i < _sessionSettings.regionSlices; i += 2)
                {
                    ApertureSlices[k] = (i * _sliceSize, (i + 1) * _sliceSize);
                    SliceLabels[k] = "Trained";
                    k++;
                }
            }
            else
            {
                startRegion = (_sessionSettings.flipRegions) ? 0 : 1;
                for (var i = startRegion; i < _sessionSettings.regionSlices; i += 2)
                {
                    ApertureSlices[i] = (i * _sliceSize, (i + 1) * _sliceSize);
                    SliceLabels[i] = "Untrained";
                }
                if(startRegion == 0)
                { startRegion = 1; }
                else
                { startRegion = 0; }
                for (var i = startRegion; i < _sessionSettings.regionSlices; i += 2)
                {
                    ApertureSlices[i] = (i * _sliceSize, (i + 1) * _sliceSize);
                    SliceLabels[i] = "Trained";
                }
            }

        

        }

        public Vector2 RandomInnerStimulusPosition(out float magnitude, out float angle, out string label)
        { //everything needs to be in radians when used in trig. check on this 
            var slice = Random.Range(0, ApertureSlices.Length);
            var (start, end) = ApertureSlices[slice];
            var mid = start + (end - start )/ 2;
            label = SliceLabels[slice];
            //var minDistance = _innerApertureRadius / Mathf.Sin(_sliceSize * Mathf.PI / 180.0f / 2);
            var minDistance = _sessionSettings.innerStimulusRadius/ Mathf.Sin(Mathf.Deg2Rad * (_sliceSize / 2)); //units of visual degrees
            
            var spawnRadius = Mathf.Tan(_sessionSettings.innerStimulusSpawnRadius * Mathf.PI / 180.0f) *
                                     _sessionSettings.stimulusDepth; // units of unity distance
            var randomRadialMagnitude = 
                Random.Range(minDistance, _sessionSettings.innerStimulusSpawnRadius - _sessionSettings.innerStimulusRadius);// units of visual degrees


            var angleOffset = Mathf.Atan(_sessionSettings.innerStimulusRadius / randomRadialMagnitude) * 180.0f / Mathf.PI;
            var randomAngle = Random.Range(mid - angleOffset, mid + angleOffset);
            //magnitude needs to be converted into length from an angle at some point before position is generated
            //what is the difference between this magnitude and the other randomRadial Magnitude???
            magnitude = Mathf.Tan(randomRadialMagnitude * Mathf.Deg2Rad) * _sessionSettings.stimulusDepth; //units of unity distance

            var randomPosition = Utility.Rotate2D(new Vector2(0.0f, magnitude), randomAngle);

            angle = randomAngle;
            return randomPosition;
        }
    }
}
