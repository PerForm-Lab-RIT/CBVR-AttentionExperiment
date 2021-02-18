using System;
using System.Collections;
using DotStimulus;
using ScriptableObjects;
using UnityEngine;
using UXF;
using Valve.VR;
using Random = UnityEngine.Random;

namespace Trial_Manager
{
    public class TrialManager : MonoBehaviour
    {
        [SerializeField] private GameObject outerStimulus;
        [SerializeField] private GameObject innerStimulus;
        [SerializeField] private GameObject stimulusSpacer;
        [SerializeField] private GameObject attentionCue;
        [SerializeField] private float stimulusSpacing;
        [SerializeField] private GameObject fixationDot;
        [SerializeField] private SessionSettings sessionSettings;
        [SerializeField] private SteamVR_Action_Boolean confirmInputAction;
        [SerializeField] private SteamVR_Action_Vector2 angleSelectAction;
        [SerializeField] private SteamVR_Input_Sources inputSource;
        [SerializeField] private ActiveLaserManager laserManager;
        [SerializeField] private GameObject correctCircle;
        [SerializeField] private GameObject userCircle;
        [SerializeField] private float fineErrorTolerance;

        [SerializeField] private SoundEffects sfx;
    
        [Serializable]
        private struct SoundEffects
        {
            public AudioClip experimentStart;
            public AudioClip success;
            public AudioClip failure;
        }
        [SerializeField] private AudioSource soundPlayer;

        private int _trialCount = 1;
        private Staircase _coherenceStaircase;
        private AperturePartition _partition;
        private StimulusSettings _innerStimulusSettings;
        private StimulusSettings _outerStimulusSettings;
        private IEnumerator _trialRoutine;

        private float _innerStimMagnitude;
        private float _innerStimAngle;
    
        private bool _inputReceived;
        private bool _waitingForInput;
        private bool _isTrialSuccessful;

        private InputData _userInput;
    
        public void OnEnable()
        {
            InitializeStimuli();
            _partition = new AperturePartition(sessionSettings, _outerStimulusSettings, _innerStimulusSettings);

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
        
        public void OnDisable()
        {
            confirmInputAction[inputSource].onStateUp -= GetUserSelection;
        }
        
        public void BeginTrial(Trial trial)
        {
            Debug.Log("CURRENT STAIRCASE: " + _coherenceStaircase.CurrentStaircaseLevel());
            soundPlayer.PlayOneShot(sfx.experimentStart);
            RandomizeInnerStimulus();
            _inputReceived = false;
            
            _trialRoutine = TrialRoutine(trial);
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
                trial.result["correct_position"] = $"({_innerStimMagnitude}, {_innerStimAngle})";
                trial.result["chosen_position"] = CalculateChosenPositionPolar(chosenPosition);
                trial.result["position_error"] = positionError;
                trial.result["coherence_range"] = _innerStimulusSettings.coherenceRange; 

                if (sessionSettings.coarseAdjustEnabled &&
                    Math.Abs(chosenAngle - _innerStimulusSettings.correctAngle) < 0.001f)
                    _isTrialSuccessful = true;
                else if (!sessionSettings.coarseAdjustEnabled &&
                         Math.Abs(chosenAngle - _innerStimulusSettings.correctAngle) < fineErrorTolerance)
                    _isTrialSuccessful = true;
                else
                    _isTrialSuccessful = false;
            }
            else
            {
                _isTrialSuccessful = false;
            }
            
            if(_isTrialSuccessful)
                _coherenceStaircase.RecordWin();
            else
                _coherenceStaircase.RecordLoss();

            if (_inputReceived && _trialCount <= sessionSettings.numTrials)
            {
                Session.instance.CurrentBlock.CreateTrial();
                _trialCount++;
            }
        
            StartCoroutine(FeedBackRoutine());
        }

        private string CalculateChosenPositionPolar(Vector3 chosenPosition)
        {
            var chosenPosition2d = new Vector2(chosenPosition.x, chosenPosition.y);
            var magnitude = Mathf.Atan(chosenPosition2d.magnitude / sessionSettings.stimulusDepth) * 180f / Mathf.PI;
            var angle = Mathf.Acos(Vector2.Dot(Vector2.up, _userInput.SelectionLocation.normalized)) * 180f / Mathf.PI;
            if (chosenPosition.x > 0)
                angle = 360 - angle;
            return $"({magnitude}, {angle})";
        }

        private void InitializeStimuli()
        {
            _innerStimulusSettings = innerStimulus.GetComponent<DotManager>().GetSettings();
            _outerStimulusSettings = outerStimulus.GetComponent<DotManager>().GetSettings();

            _innerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth - stimulusSpacing;
            _outerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth;

            _innerStimulusSettings.apertureRadiusDegrees = sessionSettings.innerStimulusRadius;
            _outerStimulusSettings.apertureRadiusDegrees = sessionSettings.outerStimulusRadius;

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
            }

            if(_isTrialSuccessful)
                soundPlayer.PlayOneShot(sfx.success);
            else
                soundPlayer.PlayOneShot(sfx.failure);

            correctCircle.SetActive(true);
        
            yield return new WaitForSeconds(sessionSettings.interTrialDelay);
        
            correctCircle.SetActive(false);
            userCircle.SetActive(false);
            
            // Redo trial if timed-out
            if(!_inputReceived)
                BeginTrial(Session.instance.CurrentTrial);
            else if (_trialCount <= sessionSettings.numTrials)
                Session.instance.BeginNextTrial();
            else
                Session.instance.End();
        }

        private IEnumerator TrialRoutine(Trial trial)
        {
            fixationDot.SetActive(true);
            // TODO: Perform fixation check using eyetracker + coroutine
            yield return new WaitForSeconds(sessionSettings.fixationTime);

            if (sessionSettings.sessionType == SessionSettings.SessionType.Training)
            {
                attentionCue.SetActive(true);
                yield return new WaitForSeconds(sessionSettings.attentionCueDuration);
                attentionCue.SetActive(false);
            }

            outerStimulus.SetActive(true);
            stimulusSpacer.SetActive(true);
            innerStimulus.SetActive(true);
            yield return new WaitForSeconds(sessionSettings.innerStimulusDuration / 1000);
        
            laserManager.ActivateLaser();
            innerStimulus.SetActive(false);
            fixationDot.SetActive(false);
            stimulusSpacer.SetActive(false);
            _waitingForInput = true;
            yield return new WaitForSeconds((sessionSettings.outerStimulusDuration - sessionSettings.innerStimulusDuration) / 1000);
            _waitingForInput = false;
            outerStimulus.SetActive(false);
            trial.End();
        }
        
        private void RandomizeInnerStimulus()
        {
            var randomPosition = _partition.RandomInnerStimulusPosition(out _innerStimMagnitude, out _innerStimAngle);
            innerStimulus.transform.localPosition =
                new Vector3(randomPosition.x, randomPosition.y, sessionSettings.stimulusDepth - stimulusSpacing);

            _innerStimulusSettings.correctAngle = sessionSettings.coarseAdjustEnabled
                ? sessionSettings.choosableAngles[Random.Range(0, sessionSettings.choosableAngles.Count)]
                : Random.Range(0.0f, 360.0f);

            _innerStimulusSettings.coherenceRange = _coherenceStaircase.CurrentStaircaseLevel();
            innerStimulus.GetComponent<DotManager>().InitializeWithSettings(_innerStimulusSettings);

            stimulusSpacer.transform.localPosition = 
                new Vector3(randomPosition.x, randomPosition.y, sessionSettings.stimulusDepth - stimulusSpacing / 2);
        }

        private struct InputData
        {
            public Vector2 SelectionLocation;
            public Vector2 ChosenDirection;
        }
    }
}
