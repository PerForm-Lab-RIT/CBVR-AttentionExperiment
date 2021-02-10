using System;
using System.Collections;
using DotStimulus;
using ScriptableObjects;
using UnityEngine;
using UXF;
using Valve.VR;
using Random = UnityEngine.Random;

public class TrialManager : MonoBehaviour
{
    [SerializeField] private GameObject outerStimulus;
    [SerializeField] private GameObject innerStimulus;
    [SerializeField] private GameObject fixationDot;
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private SteamVR_Action_Boolean confirmInputAction;
    [SerializeField] private SteamVR_Action_Vector2 angleSelectAction;
    [SerializeField] private SteamVR_Input_Sources inputSource;
    [SerializeField] private ActiveLaserManager laserManager;
    [SerializeField] private GameObject correctCircle;
    [SerializeField] private GameObject userCircle;

    [SerializeField] private SoundEffects sfx;
    
    [Serializable]
    private struct SoundEffects
    {
        public AudioClip experimentStart;
        public AudioClip attentionCue;
        public AudioClip success;
        public AudioClip failure;
    }
    [SerializeField] private AudioSource soundPlayer;

    private int _trialCount = 1;
    private Staircase _coherenceStaircase;
    private StimulusSettings _innerStimulusSettings;
    private StimulusSettings _outerStimulusSettings;
    private (float, float)[] _apertureSlices;
    private IEnumerator _trialRoutine;
    
    private bool _inputReceived;
    private bool _waitingForInput;

    private InputData _userInput;
    
    public void OnEnable()
    {
        _apertureSlices = PartitionAperture();
        InitializeStimuli();

        var fixationDotRadius = sessionSettings.fixationDotRadius * Mathf.PI / 180 * sessionSettings.stimulusDepth;
        fixationDot.transform.localScale = new Vector3(2.0f * fixationDotRadius, 0.0f, 2.0f * fixationDotRadius);
        fixationDot.transform.localPosition = new Vector3(0.0f, 0.0f, sessionSettings.stimulusDepth);
        
        correctCircle.SetActive(false);
        userCircle.SetActive(false);

        _coherenceStaircase = new Staircase(sessionSettings.coherenceStaircase, 
            sessionSettings.staircaseIncreaseThreshold, 
            sessionSettings.staircaseDecreaseThreshold);

        confirmInputAction[inputSource].onStateUp += GetUserSelection;
        soundPlayer.volume = 0.5f;
    }

    private void InitializeStimuli()
    {
        _innerStimulusSettings = innerStimulus.GetComponent<DotManager>().GetSettings();
        _outerStimulusSettings = outerStimulus.GetComponent<DotManager>().GetSettings();

        _innerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth;
        _outerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth;

        innerStimulus.GetComponent<DotManager>().InitializeWithSettings(_innerStimulusSettings);
        outerStimulus.GetComponent<DotManager>().InitializeWithSettings(_outerStimulusSettings);
    }

    private void GetUserSelection(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (_waitingForInput)
        {
            StopCoroutine(_trialRoutine);
            innerStimulus.SetActive(false);
            outerStimulus.SetActive(false);
            fixationDot.SetActive(false);

            var selectionLocation =
                outerStimulus.transform.InverseTransformPoint(laserManager.GetActiveSelectionTransform().position);

            var chosenDirection = angleSelectAction.axis.normalized;
            if (sessionSettings.coarseAdjustEnabled)
                chosenDirection = DiscretizeInput(chosenDirection);
            
            _userInput = new InputData
            {
                ChosenDirection = chosenDirection,
                SelectionLocation = new Vector2(selectionLocation.x, selectionLocation.z)
            };
            _waitingForInput = false;
            _inputReceived = true;
            Session.instance.CurrentTrial.End();
        }
    }

    private Vector2 DiscretizeInput(Vector2 chosenDirection)
    {
        var angleChoiceList = sessionSettings.choosableAngles;
        var choiceDifferences = new float[sessionSettings.choosableAngles.Count];
        
        var minimumDifference = float.MaxValue;
        var bestChoice = new Vector2();

        for (var i = 0; i < angleChoiceList.Count; i++)
        {
            var directionChoice = Utility.Rotate2D(Vector2.up, angleChoiceList[i]);
            var difference = Mathf.Acos(Vector2.Dot(directionChoice, chosenDirection));

            if (difference < minimumDifference)
            {
                bestChoice = directionChoice;
                minimumDifference = difference;
            }
        }
        return bestChoice;
    }

    private (float, float)[] PartitionAperture()
    {
        if(sessionSettings.regionSlices % 2 != 0)
            Debug.LogWarning("Odd number of aperture slices detected! Please use an even number.");
        
        var slices = new (float, float)[sessionSettings.regionSlices / 2];
        var sliceSize = 360.0f / sessionSettings.regionSlices;

        var startRegion = 0;
        if (sessionSettings.sessionType == SessionSettings.SessionType.Training)
            startRegion = (sessionSettings.flipRegions) ? 1 : 0;
        else
            startRegion = (sessionSettings.flipRegions) ? 0 : 1;
        
        var j = 0;
        for (var i = startRegion; i < sessionSettings.regionSlices; i += 2)
        {
            slices[j] = (i * sliceSize, (i + 1) * sliceSize);
            j++;
        }

        return slices;
    }

    public void BeginTrial(Trial trial)
    {
        Debug.Log("CURRENT STAIRCASE: " + _coherenceStaircase.CurrentStaircaseLevel());
        soundPlayer.PlayOneShot(sfx.experimentStart);
        var (start, end) = _apertureSlices[Random.Range(0, _apertureSlices.Length)];
        var randomAngle = Random.Range(start, end);

        var outerApertureRadius = Mathf.Tan(_outerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                             sessionSettings.stimulusDepth;
        var spaceBuffer = Mathf.Tan(_innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                          sessionSettings.stimulusDepth;
        var randomRadialMagnitude = Random.Range(spaceBuffer, outerApertureRadius - spaceBuffer);
        var randomPosition = Utility.Rotate2D(new Vector2(0.0f, randomRadialMagnitude), randomAngle);
        innerStimulus.transform.localPosition =
            new Vector3(randomPosition.x, randomPosition.y, sessionSettings.stimulusDepth);
        
        _innerStimulusSettings.correctAngle = (sessionSettings.coarseAdjustEnabled) ? 
            sessionSettings.choosableAngles[Random.Range(0, sessionSettings.choosableAngles.Count)] : Random.Range(0.0f, 360.0f);
        
        _innerStimulusSettings.coherenceRange = _coherenceStaircase.CurrentStaircaseLevel();
        innerStimulus.GetComponent<DotManager>().InitializeWithSettings(_innerStimulusSettings);
        _trialRoutine = TrialRoutine(trial);
        _inputReceived = false;
        StartCoroutine(_trialRoutine);
    }

    public void EndTrial(Trial trial)
    {
        laserManager.DeactivateBothLasers();
        
        trial.result["correct_angle"] = _innerStimulusSettings.correctAngle;

        if (_inputReceived)
        {
            var chosenAngle = Mathf.Acos(Vector2.Dot(Vector2.up, 
                _userInput.ChosenDirection.normalized)) * 180f / Mathf.PI;
            if (_userInput.ChosenDirection.x > 0)
                chosenAngle = 360.0f - chosenAngle;

            var innerStimulusPosition = innerStimulus.transform.localPosition;
            var chosenPosition = new Vector3(_userInput.SelectionLocation.x, _userInput.SelectionLocation.y,
                _outerStimulusSettings.stimDepthMeters);

            var positionError = Mathf.Acos(Vector3.Dot(innerStimulusPosition.normalized, chosenPosition.normalized)) * 180f / Mathf.PI;
            trial.result["chosen_angle"] = chosenAngle;
            trial.result["position_error"] = positionError;
            _coherenceStaircase.RecordWin();
        }
        else
        {
            trial.result["chosen_angle"] = "T/O";
            trial.result["position_error"] = "T/O";
            _coherenceStaircase.RecordLoss();
        }

        if (_trialCount < sessionSettings.numTrials)
        {
            Session.instance.CurrentBlock.CreateTrial();
            _trialCount++;
        }
        
        StartCoroutine(FeedBackRoutine());
    }

    private IEnumerator FeedBackRoutine()
    {
        var innerApertureRadius = Mathf.Tan(_innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) *
                                  _innerStimulusSettings.stimDepthMeters;
        correctCircle.transform.localPosition = innerStimulus.transform.localPosition;
        correctCircle.transform.localScale = new Vector3(2 * innerApertureRadius, 2 * innerApertureRadius, 1.0f);
        correctCircle.transform.localRotation = Quaternion.Euler(0f, 0f, _innerStimulusSettings.correctAngle);

        if (_inputReceived)
        {
            var userRotation = Mathf.Acos(Vector2.Dot(Vector3.up, _userInput.ChosenDirection.normalized))
                * 180f / Mathf.PI;
            if (_userInput.ChosenDirection.x > 0)
                userRotation = -userRotation;
            userCircle.transform.localPosition = new Vector3(_userInput.SelectionLocation.x,
                _userInput.SelectionLocation.y, _outerStimulusSettings.stimDepthMeters);
            userCircle.transform.localScale = new Vector3(2 * innerApertureRadius, 2 * innerApertureRadius, 1.0f);
            userCircle.transform.localRotation = Quaternion.Euler(0f, 0f, userRotation);
            
            userCircle.SetActive(true);
            soundPlayer.PlayOneShot(sfx.success);
        }
        else
        {
            soundPlayer.PlayOneShot(sfx.failure);
        }
        
        correctCircle.SetActive(true);
        
        yield return new WaitForSeconds(sessionSettings.interTrialDelay);
        
        correctCircle.SetActive(false);
        userCircle.SetActive(false);
        
        Session.instance.BeginNextTrialSafe();
    }

    private IEnumerator TrialRoutine(Trial trial)
    {
        fixationDot.SetActive(true);
        yield return new WaitForSeconds(sessionSettings.fixationTime);
        
        fixationDot.SetActive(false);
        outerStimulus.SetActive(true);
        innerStimulus.SetActive(true);
        yield return new WaitForSeconds(sessionSettings.innerStimulusDuration / 1000);
        
        laserManager.ActivateLaser();
        innerStimulus.SetActive(false);
        _waitingForInput = true;
        yield return new WaitForSeconds((sessionSettings.outerStimulusDuration - sessionSettings.innerStimulusDuration) / 1000);
        _waitingForInput = false;
        outerStimulus.SetActive(false);
        trial.End();
    }

    public void OnDisable()
    {
        confirmInputAction[inputSource].onStateUp -= GetUserSelection;
    }

    private struct InputData
    {
        public Vector2 SelectionLocation;
        public Vector2 ChosenDirection;
    }
}
