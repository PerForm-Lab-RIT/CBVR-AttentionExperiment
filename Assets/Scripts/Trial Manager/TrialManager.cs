using System;
using System.Collections;
using DotStimulus;
using ScriptableObjects;
using ScriptableObjects.Variables;
using UnityEngine;
using UXF;
using Valve.VR;
using Random = UnityEngine.Random;

namespace Trial_Manager
{
    public class TrialManager : MonoBehaviour
    {
        [SerializeField] private SessionSettings sessionSettings;
        
        [Header("Stimuli")]
        [SerializeField] private GameObject outerStimulus;
        [SerializeField] private GameObject innerStimulus;
        [SerializeField] private GameObject stimulusSpacer;
        
        [Header("Input")]
        [SerializeField] private SteamVR_Action_Boolean confirmInputAction;
        [SerializeField] private SteamVR_Action_Vector2 angleSelectAction;
        [SerializeField] private SteamVR_Input_Sources inputSource;
        [SerializeField] private ActiveLaserManager laserManager;
        
        [Header("Sound")]
        [SerializeField] private GameObject attentionCue;
        [SerializeField] private SoundPlayer soundPlayer;
        
        [Header("Events")]
        [SerializeField] private GameEvent staircaseUpdateEvent;
        [SerializeField] private GameEvent trialNumUpdateEvent;
        [SerializeField] private GameEvent percentageCorrectUpdateEvent;
        
        [Header("Variables")]
        [SerializeField] private IntVariable trialCount;
        [SerializeField] private FloatVariable correctPercentage;
        [SerializeField] private StringVariable staircaseType;
        [SerializeField] private IntVariable staircaseLevel;
        
        [Header("Misc")]
        [SerializeField] private GameObject fixationDot;
        [SerializeField] private FeedbackModule feedbackModule;
        [SerializeField] private SelectEyeTracker eyeTrackerSelector;
        [SerializeField] private Transform cameraTransform;
        
        private int _numCorrect;
        private int _totalNumTrials;

        private AperturePartition _partition;
        private StimulusSettings _innerStimulusSettings;
        private StimulusSettings _outerStimulusSettings;
        private IEnumerator _trialRoutine;

        private float _innerStimMagnitude;
        private float _innerStimAngle;
        
        public bool IsPausable { get; private set; }
    
        private bool _inputReceived;
        private bool _waitingForInput;
        private bool _isTrialSuccessful;
        private bool _isFixationBroken;

        private InputData _userInput;
        private DotManager _innerStimulusManager;

        [ReadOnly] [SerializeField] private Vector3 enforcedHeadPosition;
        [ReadOnly] [SerializeField] private Quaternion enforcedHeadRotation;
        
        // Property: StaircaseManager
        // Handles the management and interleaving of staircases
        public StaircaseManager StaircaseManager { get; private set; }

        public void Start()
        {
            trialCount.value = 1;
            correctPercentage.value = 100.0f;
            _totalNumTrials = sessionSettings.numTrials;
            IsPausable = false;
        }
        
        public void OnEnable()
        {
            InitializeStimuli();
            InitializeFixationDot();
            _innerStimulusManager = innerStimulus.GetComponent<DotManager>();
            _partition = new AperturePartition(sessionSettings, _outerStimulusSettings, _innerStimulusSettings);
            StaircaseManager = new StaircaseManager(sessionSettings);
            SetEnforcedHeadTransform();

            confirmInputAction[inputSource].onStateUp += GetUserSelection;
        }
        
        public void OnDisable()
        {
            confirmInputAction[inputSource].onStateUp -= GetUserSelection;
        }
        
        // Called via UXF event
        public void BeginTrial(Trial trial)
        {
            StaircaseManager.RandomizeStaircase();
            staircaseType.value = StaircaseManager.CurrentStaircaseName();
            staircaseLevel.value = StaircaseManager.CurrentStaircase.CurrentStaircaseLevel();
            staircaseUpdateEvent.Raise();
            trialNumUpdateEvent.Raise();
            percentageCorrectUpdateEvent.Raise();
            
            _isTrialSuccessful = false;
            _isFixationBroken = false;
            soundPlayer.PlayStartSound();
            RandomizeInnerStimulus();
            _inputReceived = false;
            
            _trialRoutine = TrialRoutine(trial);
            StartCoroutine(_trialRoutine);
        }

        public void SetEnforcedHeadTransform()
        {
            enforcedHeadPosition = cameraTransform.position;
            enforcedHeadRotation = cameraTransform.rotation;
        }

        // Called via UXF event
        public void EndTrial(Trial trial)
        {
            laserManager.DeactivateBothLasers();
            
            if (_inputReceived)
            {
                CalculateOutputs(out var chosenAngle, out var chosenPosition, out var positionError);
                if (positionError < sessionSettings.positionErrorTolerance)
                {
                    if (StaircaseManager.CheckForLocationWin())
                    {
                        _isTrialSuccessful = true;
                        _numCorrect++;
                    }
                }
                else
                    StaircaseManager.CheckForLocationLoss();

                if (Math.Abs(chosenAngle - _innerStimulusSettings.correctAngle) < sessionSettings.angleErrorTolerance)
                {
                    if (StaircaseManager.CheckForDirectionWin())
                    {
                        _isTrialSuccessful = true;
                        _numCorrect++;
                    }
                }
                else
                    StaircaseManager.CheckForDirectionLoss();
                RecordTrialData(trial, chosenAngle, chosenPosition, positionError);
            }

            correctPercentage.value = Convert.ToSingle(_numCorrect) / Convert.ToSingle(trialCount.value) * 100.0f;
            percentageCorrectUpdateEvent.Raise();
            
            if (_inputReceived || sessionSettings.timeoutIsFailure)
                trialCount.value++;

            StartCoroutine(FeedBackRoutine());
        }

        private void InitializeFixationDot()
        {
            var fixationDotRadius = sessionSettings.fixationDotRadius * Mathf.PI / 180 * sessionSettings.stimulusDepth;
            fixationDot.transform.localScale = new Vector3(2.0f * fixationDotRadius, 0.0f, 2.0f * fixationDotRadius);
            fixationDot.transform.localPosition = new Vector3(0.0f, 0.0f, sessionSettings.stimulusDepth);
        }
        
        private void CalculateOutputs(out float chosenAngle, out Vector3 chosenPosition, out float positionError)
        {
            chosenAngle = Mathf.Acos(Vector2.Dot(Vector2.up,
                _userInput.chosenDirection.normalized)) * 180f / Mathf.PI;
            if (_userInput.chosenDirection.x > 0)
                chosenAngle = 360.0f - chosenAngle;

            var innerStimulusPosition = innerStimulus.transform.localPosition;
            chosenPosition = new Vector3(_userInput.selectionLocation.x, _userInput.selectionLocation.y,
                _outerStimulusSettings.stimDepthMeters);

            positionError = Mathf.Acos(Vector3.Dot(innerStimulusPosition.normalized, chosenPosition.normalized)) * 180f /
                            Mathf.PI;
        }

        private void RecordTrialData(Trial trial, float chosenAngle, Vector3 chosenPosition, float positionError)
        {
            trial.result["correct_angle"] = _innerStimulusSettings.correctAngle;
            trial.result["chosen_angle"] = chosenAngle;
            trial.result["correct_position_magnitude"] = _innerStimMagnitude;
            trial.result["correct_position_angle"] = _innerStimAngle;
            var (magnitude, angle) = CalculateChosenPositionPolar(chosenPosition);
            trial.result["chosen_position_magnitude"] = magnitude;
            trial.result["chosen_position_angle"] = angle;
            trial.result["position_error"] = positionError;
            trial.result["coherence_range"] = _innerStimulusSettings.coherenceRange;
            trial.result["position_within_threshold"] = positionError < sessionSettings.positionErrorTolerance;
            trial.result["angle_within_threshold"] = _isTrialSuccessful;
            trial.result["staircase"] = StaircaseManager.CurrentStaircaseName();
        }

        private (float magnitude, float angle) CalculateChosenPositionPolar(Vector3 chosenPosition)
        {
            var chosenPosition2d = new Vector2(chosenPosition.x, chosenPosition.y);
            var magnitude = Mathf.Atan(chosenPosition2d.magnitude / sessionSettings.stimulusDepth) * 180f / Mathf.PI;
            var angle = Mathf.Acos(Vector2.Dot(Vector2.up, _userInput.selectionLocation.normalized)) * 180f / Mathf.PI;
            if (chosenPosition.x > 0)
                angle = 360 - angle;
            return (magnitude, angle);
        }

        private void InitializeStimuli()
        {
            _innerStimulusSettings = innerStimulus.GetComponent<DotManager>().GetSettings();
            _outerStimulusSettings = outerStimulus.GetComponent<DotManager>().GetSettings();
            _innerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth - sessionSettings.stimulusSpacing;
            _outerStimulusSettings.stimDepthMeters = sessionSettings.stimulusDepth;
            _innerStimulusSettings.apertureRadiusDegrees = sessionSettings.innerStimulusRadius;
            _outerStimulusSettings.apertureRadiusDegrees = sessionSettings.outerStimulusRadius;
            _innerStimulusSettings.density = sessionSettings.stimulusDensity;
            _outerStimulusSettings.density = sessionSettings.stimulusDensity;
            _innerStimulusSettings.minDotLifetime = sessionSettings.minDotLifetime;
            _outerStimulusSettings.minDotLifetime = sessionSettings.minDotLifetime;
            _innerStimulusSettings.maxDotLifetime = sessionSettings.maxDotLifetime;
            _outerStimulusSettings.maxDotLifetime = sessionSettings.maxDotLifetime;
            _innerStimulusSettings.dotSizeArcMinutes = sessionSettings.dotSize;
            _outerStimulusSettings.dotSizeArcMinutes = sessionSettings.dotSize;
            _innerStimulusSettings.noiseDotPercentage = sessionSettings.innerStimulusNoisePercentage;
            _outerStimulusSettings.noiseDotPercentage = sessionSettings.outerStimulusNoisePercentage;
            _innerStimulusSettings.buddyDotsEnabled = sessionSettings.buddyDotsEnabled;
            _outerStimulusSettings.buddyDotsEnabled = sessionSettings.buddyDotsEnabled;

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
                    chosenDirection = chosenDirection,
                    selectionLocation = new Vector2(selectionLocation.x, selectionLocation.z)
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

            foreach (var angleChoice in angleChoiceList)
            {
                var directionChoice = Utility.Rotate2D(Vector2.up, angleChoice);
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
            feedbackModule.GiveFeedback(_inputReceived, _isTrialSuccessful, _userInput);
            yield return new WaitForSeconds(sessionSettings.interTrialDelay);
            feedbackModule.HideFeedback();

            // Redo trial if timed-out
            if (!_inputReceived && !sessionSettings.timeoutIsFailure)
            {
                BeginTrial(Session.instance.CurrentTrial);
                yield break;
            }

            if (!_inputReceived)
                _totalNumTrials++;
            
            if (trialCount.value <= _totalNumTrials)
            {
                Session.instance.CurrentBlock.CreateTrial();
                Session.instance.BeginNextTrial();
            }
            else
                Session.instance.End();
        }

        private IEnumerator TrialRoutine(Trial trial)
        {
            fixationDot.SetActive(true);
            
            IsPausable = true;
            yield return WaitForFixation(sessionSettings.fixationTime, 
                Mathf.Tan(sessionSettings.fixationErrorTolerance * Mathf.PI / 180 * sessionSettings.stimulusDepth));
            IsPausable = false;

            var trialDuration = GetTrialDuration();

            var outerStimulusRoutine = StartCoroutine(OuterStimulusRoutine());
            var innerStimulusRoutine = StartCoroutine(InnerStimulusRoutine());
            var inputRoutine = StartCoroutine(InputRoutine());
            var fixationBreakCheckRoutine = StartCoroutine(FixationBreakCheckRoutine());
            var headStabilityCheckRoutine = StartCoroutine(HeadStabilityCheckRoutine());
            var attentionCueRoutine = AttentionCueRoutine();
            
            if (sessionSettings.sessionType == SessionSettings.SessionType.Training)
                StartCoroutine(attentionCueRoutine);

            var elapsedTime = 0.0f;
            while (!_isFixationBroken && elapsedTime < trialDuration / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            StopCoroutine(outerStimulusRoutine);
            StopCoroutine(innerStimulusRoutine);
            StopCoroutine(inputRoutine);
            if(fixationBreakCheckRoutine != null) StopCoroutine(fixationBreakCheckRoutine);
            StopCoroutine(attentionCueRoutine);
            StopCoroutine(headStabilityCheckRoutine);
            
            outerStimulus.SetActive(false);
            innerStimulus.SetActive(false);
            stimulusSpacer.SetActive(false);
            attentionCue.SetActive(false);
            laserManager.DeactivateBothLasers();

            // Start trial over if fixation was broken
            if (_isFixationBroken)
            {
                BeginTrial(trial);
                yield break;
            }
            
            trial.End();
        }

        public void Pause()
        {
            StopCoroutine(_trialRoutine);
            fixationDot.SetActive(false);
        }

        private float GetTrialDuration()
        {
            var endTimes = new[]
            {
                sessionSettings.outerStimulusStart + sessionSettings.outerStimulusDuration,
                sessionSettings.innerStimulusStart + sessionSettings.innerStimulusDuration,
                sessionSettings.fixationBreakCheckStart + sessionSettings.fixationBreakCheckDuration,
                sessionSettings.inputStart + sessionSettings.inputDuration
            };
            
            return Mathf.Max(endTimes);
        }

        private IEnumerator InputRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.inputStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _waitingForInput = true;
            laserManager.ActivateLaser();
            
            elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.inputDuration / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            laserManager.DeactivateBothLasers();
            _waitingForInput = false;
        }
        
        private IEnumerator OuterStimulusRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.outerStimulusStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0.0f;
            outerStimulus.SetActive(true);
            while (elapsedTime < sessionSettings.outerStimulusDuration / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            outerStimulus.SetActive(false);
        }
        
        private IEnumerator InnerStimulusRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.innerStimulusStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            innerStimulus.SetActive(true);
            stimulusSpacer.SetActive(true);

            elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.innerStimulusDuration / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            innerStimulus.SetActive(false);
            yield return null;
            stimulusSpacer.SetActive(false);
        }
        
        private IEnumerator AttentionCueRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.attentionCueStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            attentionCue.SetActive(true);
            
            elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.attentionCueDuration / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            attentionCue.SetActive(false);
        }

        private IEnumerator FixationBreakCheckRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.fixationBreakCheckStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.fixationBreakCheckDuration / 1000)
            {
                if (Physics.Raycast(cameraTransform.position,
                    cameraTransform.TransformDirection(eyeTrackerSelector.ChosenTracker.GetLocalGazeDirection()), out var hit))
                {
                    Debug.DrawRay(cameraTransform.position,
                        hit.distance * cameraTransform.TransformDirection(eyeTrackerSelector.ChosenTracker.GetLocalGazeDirection()),
                        Color.yellow);
                    var fixationError = Mathf.Tan(sessionSettings.fixationErrorTolerance * Mathf.PI / 180 *
                                                  sessionSettings.stimulusDepth);
                    if ((hit.point - fixationDot.transform.position).magnitude > fixationError)
                    {
                        innerStimulus.SetActive(false);
                        outerStimulus.SetActive(false);
                        _isFixationBroken = true;
                        yield break;
                    }
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator HeadStabilityCheckRoutine()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.headFixationStart / 1000)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0.0f;
            while (elapsedTime < sessionSettings.headFixationDuration / 1000)
            {
                var sqrDistance = (cameraTransform.localPosition - enforcedHeadPosition).sqrMagnitude;
                var angularDifference = Quaternion.Angle(cameraTransform.rotation, enforcedHeadRotation);
                if (sqrDistance > sessionSettings.headFixationDistanceErrorTolerance * sessionSettings.headFixationDistanceErrorTolerance || 
                    angularDifference > sessionSettings.headFixationAngleErrorTolerance)
                {
                    innerStimulus.SetActive(false);
                    outerStimulus.SetActive(false);
                    _isFixationBroken = true;
                    yield break;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator WaitForFixation(float fixationTime, float maxFixationError)
        {
            var timeFixated = 0.0f;
            while (timeFixated < fixationTime)
            {
                timeFixated += Time.deltaTime;
                
                // Gaze Fixation check
                if (Physics.Raycast(cameraTransform.position, 
                    cameraTransform.TransformDirection(eyeTrackerSelector.ChosenTracker.GetLocalGazeDirection()), out var hit))
                {
                    Debug.DrawRay(cameraTransform.position, 
                        hit.distance * cameraTransform.TransformDirection(eyeTrackerSelector.ChosenTracker.GetLocalGazeDirection()), Color.yellow);
                    if ((hit.point - fixationDot.transform.position).magnitude > maxFixationError)
                        timeFixated = 0.0f;
                }
                else
                {
                    timeFixated = 0.0f;
                }
                
                // Head Fixation check
                var sqrDistance = (cameraTransform.localPosition - enforcedHeadPosition).sqrMagnitude;
                var angularDifference = Quaternion.Angle(cameraTransform.rotation, enforcedHeadRotation);
                if (sqrDistance > sessionSettings.headFixationDistanceErrorTolerance * sessionSettings.headFixationDistanceErrorTolerance || 
                    angularDifference > sessionSettings.headFixationAngleErrorTolerance)
                {
                    timeFixated = 0.0f;
                }
                
                yield return null;
            }
        }

        private void RandomizeInnerStimulus()
        {
            var randomPosition = _partition.RandomInnerStimulusPosition(out _innerStimMagnitude, out _innerStimAngle);
            innerStimulus.transform.localPosition =
                new Vector3(randomPosition.x, randomPosition.y, sessionSettings.stimulusDepth - sessionSettings.stimulusSpacing);

            _innerStimulusSettings.correctAngle = sessionSettings.coarseAdjustEnabled
                ? sessionSettings.choosableAngles[Random.Range(0, sessionSettings.choosableAngles.Count)]
                : Random.Range(0.0f, 360.0f);

            _innerStimulusSettings.coherenceRange = StaircaseManager.CurrentStaircase.CurrentStaircaseAngle();
            _innerStimulusManager.InitializeWithSettings(_innerStimulusSettings);
        }
    }
}
