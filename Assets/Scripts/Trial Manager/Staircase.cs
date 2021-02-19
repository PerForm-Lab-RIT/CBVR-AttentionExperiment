using System;
using System.Collections.Generic;

namespace Trial_Manager
{
    public class Staircase
    {
        private readonly List<float> _staircaseLevels;
        private int _currentStaircase;
    
        private readonly int _increaseThreshold;
        private readonly int _decreaseThreshold;

        private uint _winStreak;
        private uint _loseStreak;

        public Staircase(List<float> staircaseLevels, int increaseThreshold, int decreaseThreshold)
        {
            _staircaseLevels = staircaseLevels;
            _increaseThreshold = increaseThreshold;
            _decreaseThreshold = decreaseThreshold;
            _currentStaircase = 0;
            _winStreak = 0;
            _loseStreak = 0;
        }

        public float CurrentStaircaseLevel()
        {
            return _staircaseLevels[_currentStaircase];
        }
        
        public void RecordWin()
        {
            _winStreak++;
            _loseStreak = 0;
            if (_currentStaircase < _staircaseLevels.Count - 1 && _winStreak >= _increaseThreshold)
            {
                _currentStaircase++;
                _winStreak = 0;
            }
        }

        public void RecordLoss()
        {
            _loseStreak++;
            _winStreak = 0;
            if (_currentStaircase > 0 && _loseStreak >= _decreaseThreshold)
            {
                _currentStaircase--;
                _loseStreak = 0;
            }
        }
    }
}
