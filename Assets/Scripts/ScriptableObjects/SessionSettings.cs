using System;
using System.Collections.Generic;
using System.Linq;
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
        public float outerStimulusRadius;
        public float innerStimulusRadius;
        public float outerStimulusDuration;
        public float innerStimulusDuration;
        public float stimulusDepth;
        public float interTrialDelay;
        
        public List<float> coherenceStaircase;
        public int staircaseIncreaseThreshold;
        public int staircaseDecreaseThreshold;
        
        public int regionSlices;
        public bool flipRegions;

        public bool coarseAdjustEnabled;
        public List<float> choosableAngles;

        // IMPORTANT: Any changes made in this function should be cross-checked with both the corresponding JSON
        // and the UXF data-points collection
        public void LoadFromUxfJson()
        {
            var sessionSettingsDict = Session.instance.settings.GetDict("SessionSettings");
            
            sessionType = ParseSessionType((string) Session.instance.participantDetails["SessionType"]);
            numTrials = Convert.ToInt32(sessionSettingsDict["NumTrials"]);
            fixationTime = Convert.ToSingle(sessionSettingsDict["FixationTimeInSeconds"]);
            fixationDotRadius = Convert.ToSingle(sessionSettingsDict["FixationDotRadiusDegrees"]);
            skyColor = ParseColor((List<object>) sessionSettingsDict["SkyColor"]);
            outerStimulusRadius = Convert.ToSingle(sessionSettingsDict["OuterStimulusRadiusDegrees"]);
            innerStimulusRadius = Convert.ToSingle(sessionSettingsDict["InnerStimulusRadiusDegrees"]);
            outerStimulusDuration = Convert.ToSingle(sessionSettingsDict["OuterStimulusDurationMs"]);
            innerStimulusDuration = Convert.ToSingle(sessionSettingsDict["InnerStimulusDurationMs"]);
            stimulusDepth = Convert.ToSingle(sessionSettingsDict["StimulusDepthMeters"]);
            interTrialDelay = Convert.ToSingle(sessionSettingsDict["InterTrialDelaySeconds"]);
 
            regionSlices = Convert.ToInt32(sessionSettingsDict["TotalRegionSlices"]);
            flipRegions = Convert.ToBoolean(sessionSettingsDict["FlipRegions"]);
            
            coherenceStaircase = ParseFloatList((List<object>) sessionSettingsDict["CoherenceStaircase"]);
            staircaseIncreaseThreshold = Convert.ToInt32(sessionSettingsDict["StaircaseIncreaseThreshold"]);
            staircaseDecreaseThreshold = Convert.ToInt32(sessionSettingsDict["StaircaseDecreaseThreshold"]);
            interTrialDelay = Convert.ToSingle(sessionSettingsDict["InterTrialDelaySeconds"]);
            
            coarseAdjustEnabled = Convert.ToBoolean(sessionSettingsDict["CoarseAdjustment"]);
            choosableAngles = ParseFloatList((List<object>) sessionSettingsDict["ChoosableAngles"]);
        }

        private static List<float> ParseFloatList(IEnumerable<object> list)
        {
            return list.Select(Convert.ToSingle).ToList();
        }

        private static Color ParseColor(IReadOnlyList<object> color)
        {
            return new Color(
                Convert.ToSingle(color[0]),
                Convert.ToSingle(color[1]),
                Convert.ToSingle(color[2])
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
