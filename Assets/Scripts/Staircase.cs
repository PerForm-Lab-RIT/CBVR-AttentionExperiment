﻿using System.Collections.Generic;

public class Staircase
{
    private readonly List<float> _staircaseLevels;
    private int _currentStaircase;
    
    private readonly uint _increaseThreshold;
    private readonly uint _decreaseThreshold;

    private uint _winStreak;
    private uint _loseStreak;
    
    public Staircase(List<float> staircaseLevels, uint increaseThreshold, uint decreaseThreshold)
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
        if (_currentStaircase < _staircaseLevels.Count && _winStreak >= _increaseThreshold)
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
