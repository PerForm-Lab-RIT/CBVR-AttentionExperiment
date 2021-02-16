﻿using ScriptableObjects;
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
    
        public AperturePartition(SessionSettings sessionSettings, 
            StimulusSettings outerStimulusSettings, StimulusSettings innerStimulusSettings)
        {
            _sessionSettings = sessionSettings;

            _outerApertureRadius = Mathf.Tan(outerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                                   sessionSettings.stimulusDepth;
            _innerApertureRadius = Mathf.Tan(innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                                   sessionSettings.stimulusDepth;
        
            ApertureSlices = new (float, float)[_sessionSettings.regionSlices / 2];
            _sliceSize = 360.0f / _sessionSettings.regionSlices;
            PartitionAperture();
        }
    
        private void PartitionAperture()
        {
            if(_sessionSettings.regionSlices % 2 != 0)
                Debug.LogWarning("Odd number of aperture slices detected! Please use an even number.");
        
            int startRegion;
            if (_sessionSettings.sessionType == SessionSettings.SessionType.Training)
                startRegion = (_sessionSettings.flipRegions) ? 1 : 0;
            else
                startRegion = (_sessionSettings.flipRegions) ? 0 : 1;
        
            var j = 0;
            for (var i = startRegion; i < _sessionSettings.regionSlices; i += 2)
            {
                ApertureSlices[j] = (i * _sliceSize, (i + 1) * _sliceSize);
                j++;
            }
        }

        public Vector2 RandomInnerStimulusPosition()
        {
            var (start, end) = ApertureSlices[Random.Range(0, ApertureSlices.Length)];
            var minDistance = _innerApertureRadius / Mathf.Sin(_sliceSize * Mathf.PI / 180.0f / 2);
            var randomRadialMagnitude = Random.Range(minDistance, _outerApertureRadius - _innerApertureRadius);
        
            var angleOffset = Mathf.Asin(_innerApertureRadius / randomRadialMagnitude) * 180.0f / Mathf.PI;
            var randomAngle = Random.Range(start + angleOffset, end - angleOffset);
        
            var randomPosition = Utility.Rotate2D(new Vector2(0.0f, randomRadialMagnitude), randomAngle);
            return randomPosition;
        }
    }
}