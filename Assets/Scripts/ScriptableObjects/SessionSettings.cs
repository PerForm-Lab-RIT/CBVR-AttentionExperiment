using System;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Settings/Session Settings")]
    public class SessionSettings : ScriptableObject
    {
        public enum SessionType
        {
            Training,
            Testing
        }

        public SessionType sessionType;
        public int numTrials;
        public float fixationTime;
        public float fixationDotRadius;
        public Color skyColor;
        public float outerStimulusDuration;
        public float innerStimulusDuration;
        public float stimulusDepth;
        public float interTrialDelay;
        
        public int regionSlices;
        public bool flipRegions;

        // IMPORTANT: Any changes made in this function should be cross-checked with both the corresponding JSON
        // and the UXF data-points collection
        public SessionSettings LoadFromUxfJson()
        {
            var sessionSettingsDict = Session.instance.settings.GetDict("SessionSettings");
            
            sessionType = ParseSessionType((string) Session.instance.participantDetails["SessionType"]);
            numTrials = Convert.ToInt32(sessionSettingsDict["NumTrials"]);
            fixationTime = (float) Convert.ToDouble(sessionSettingsDict["FixationTimeInSeconds"]);
            fixationDotRadius = (float) Convert.ToDouble(sessionSettingsDict["FixationDotRadiusDegrees"]);
            skyColor = ParseColor((List<object>) sessionSettingsDict["SkyColor"]);
            outerStimulusDuration = (float) Convert.ToDouble(sessionSettingsDict["OuterStimulusDurationMs"]);
            innerStimulusDuration = (float) Convert.ToDouble(sessionSettingsDict["InnerStimulusDurationMs"]);
            stimulusDepth = (float) Convert.ToDouble(sessionSettingsDict["StimulusDepthMeters"]);
            interTrialDelay = (float) Convert.ToDouble(sessionSettingsDict["InterTrialDelaySeconds"]);
 
            regionSlices = Convert.ToInt32(sessionSettingsDict["TotalRegionSlices"]);
            flipRegions = (bool) sessionSettingsDict["FlipRegions"];
            return this;
        }

        private Color ParseColor(List<object> color)
        {
            return new Color(
                (float) (double) color[0],
                (float) (double) color[1],
                (float) (double) color[2]
            );
        }

        private static SessionType ParseSessionType(string sessionTypeString)
        {
            switch (sessionTypeString)
            {
                case "Training":
                    return SessionType.Training;
                case "Testing":
                    return SessionType.Testing;
                default:
                    return SessionType.Training;
            }
        }
    }
}
