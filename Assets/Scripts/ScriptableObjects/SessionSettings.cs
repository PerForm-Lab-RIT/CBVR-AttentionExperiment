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
        
        public enum CueType
        {
            Neutral,
            FeatureBased
        }

        public enum FeedbackType
        {
            Directional,
            Locational
        }

        public SessionType sessionType;
        public CueType cueType;
        public FeedbackType feedbackType;
        public int numTrials;
        public float fixationTime;
        public float fixationDotRadius;
        public Color skyColor;
        public float stimulusDensity;
        public float dotLifetime;
        public float dotSize;
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

        public float attentionCueDuration;
        public float attentionCueDepth;
        public float attentionCueDistance;
        public float pulseFrequency;
        public float sampleRate;
        public float angleErrorTolerance;
        public float positionErrorTolerance;
        public bool positionStaircaseEnabled;
        public bool directionStaircaseEnabled;

        // IMPORTANT: Any changes made in this function should be cross-checked with both the corresponding JSON
        // and the UXF data-points collection
        public void LoadFromUxfJson()
        {
            var sessionSettingsDict = Session.instance.settings.GetDict("SessionSettings");
            
            sessionType = ParseSessionType((string) Session.instance.participantDetails["SessionType"]);
            cueType = ParseCueType((string) Session.instance.participantDetails["CueType"]);
            feedbackType = ParseFeedbackType((string) Session.instance.participantDetails["FeedbackType"]);
            numTrials = Convert.ToInt32(sessionSettingsDict["NumTrials"]);
            fixationTime = Convert.ToSingle(sessionSettingsDict["FixationTimeInSeconds"]);
            fixationDotRadius = Convert.ToSingle(sessionSettingsDict["FixationDotRadiusDegrees"]);
            skyColor = ParseColor((List<object>) sessionSettingsDict["SkyColor"]);
            stimulusDensity = Convert.ToSingle(sessionSettingsDict["StimulusDensity"]);
            dotLifetime = Convert.ToSingle(sessionSettingsDict["DotLifetimeSeconds"]);
            dotSize = Convert.ToSingle(sessionSettingsDict["StimulusDotSizeArcMinutes"]);
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

            attentionCueDuration = Convert.ToSingle(sessionSettingsDict["AttentionCueDuration"]);
            attentionCueDepth = Convert.ToSingle(sessionSettingsDict["AttentionCueDepth"]);
            attentionCueDistance = Convert.ToSingle(sessionSettingsDict["AttentionCueLengthDegrees"]);
            pulseFrequency = Convert.ToSingle(sessionSettingsDict["PulseFrequency"]);
            sampleRate = Convert.ToSingle(sessionSettingsDict["SampleRate"]);

            angleErrorTolerance = Convert.ToSingle(sessionSettingsDict["AngleErrorToleranceDegrees"]);
            positionErrorTolerance = Convert.ToSingle(sessionSettingsDict["PositionErrorToleranceDegrees"]);
            
            positionStaircaseEnabled = Convert.ToBoolean(Session.instance.participantDetails["LocationalStaircaseEnabled"]);
            directionStaircaseEnabled = Convert.ToBoolean(Session.instance.participantDetails["DirectionalStaircaseEnabled"]);
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
        
        private static CueType ParseCueType(string cueTypeString)
        {
            switch (cueTypeString)
            {
                case "Neutral":
                    return CueType.Neutral;
                case "Feature-based":
                    return CueType.FeatureBased;
                default:
                    return CueType.Neutral;
            }
        }
        
        private static FeedbackType ParseFeedbackType(string feedbackTypeString)
        {
            switch (feedbackTypeString)
            {
                case "Directional":
                    return FeedbackType.Directional;
                case "Locational":
                    return FeedbackType.Locational;
                default:
                    return FeedbackType.Directional;
            }
        }
    }
}
