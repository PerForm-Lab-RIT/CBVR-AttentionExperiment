using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Trial_Manager
{
    public class StaircaseManager
    {
        private readonly SessionSettings _sessionSettings;
        private readonly Staircase _directionStaircase;
        private readonly Staircase _locationStaircase;
        public Staircase CurrentStaircase { get; private set; }

        public StaircaseManager(SessionSettings sessionSettings)
        {
            _sessionSettings = sessionSettings;
            
            _directionStaircase = new Staircase(sessionSettings.coherenceStaircase,
                sessionSettings.staircaseIncreaseThreshold,
                sessionSettings.staircaseDecreaseThreshold);

            _locationStaircase = new Staircase(sessionSettings.coherenceStaircase,
                sessionSettings.staircaseIncreaseThreshold,
                sessionSettings.staircaseDecreaseThreshold);
            CurrentStaircase = _directionStaircase;
            if (!sessionSettings.directionStaircaseEnabled && !sessionSettings.positionStaircaseEnabled)
                Debug.LogWarning("No staircase is enabled! Please enable one in the JSON settings.");
        }
        
        public bool CheckForLocationWin()
        {
            if (CurrentStaircase == _locationStaircase)
                CurrentStaircase.RecordWin();
            return _sessionSettings.feedbackType == SessionSettings.FeedbackType.Locational;
        }
        
        public void CheckForLocationLoss()
        {
            if (CurrentStaircase == _locationStaircase)
                CurrentStaircase.RecordLoss();
        }
        
        public bool CheckForDirectionWin()
        {
            if (CurrentStaircase == _directionStaircase)
                CurrentStaircase.RecordWin();
            return _sessionSettings.feedbackType == SessionSettings.FeedbackType.Directional;
        }
        
        public void CheckForDirectionLoss()
        {
            if (CurrentStaircase == _directionStaircase)
                CurrentStaircase.RecordLoss();
        }
        
        public void RandomizeStaircase()
        {
            var possibleStaircases = new List<Staircase>();
            if(_sessionSettings.directionStaircaseEnabled)
                possibleStaircases.Add(_directionStaircase);
            if(_sessionSettings.positionStaircaseEnabled)
                possibleStaircases.Add(_locationStaircase);
            CurrentStaircase = possibleStaircases[Random.Range(0, possibleStaircases.Count)];
        }

        public string CurrentStaircaseName()
        {
            return CurrentStaircase == _locationStaircase ? "Location" : "Direction";
        }
    }
}
