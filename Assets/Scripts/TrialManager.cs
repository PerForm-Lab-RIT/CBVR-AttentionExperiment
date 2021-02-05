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

    private int _trialCount = 1;
    private StimulusSettings _innerStimulusSettings;
    private StimulusSettings _outerStimulusSettings;
    private (float, float)[] _apertureSlices;
    private IEnumerator _trialRoutine;
    
    private bool _inputReceived;
    private bool _waitingForInput;

    private InputData _userInput;
    
    public void OnEnable()
    {
        if(sessionSettings.regionSlices % 2 != 0)
            Debug.LogWarning("Odd number of aperture slices detected! Please use an even number.");
        
        _apertureSlices = PartitionAperture();
        _innerStimulusSettings = innerStimulus.GetComponent<DotManager>().GetSettings();
        _outerStimulusSettings = outerStimulus.GetComponent<DotManager>().GetSettings();

        var fixationDotRadius = sessionSettings.fixationDotRadius * Mathf.PI / 180 * sessionSettings.stimulusDepth;
        fixationDot.transform.localScale = new Vector3(2.0f * fixationDotRadius, 0.0f, 2.0f * fixationDotRadius);
        fixationDot.transform.localPosition = new Vector3(0.0f, 0.0f, sessionSettings.stimulusDepth);

        confirmInputAction[inputSource].onStateUp += GetUserSelection;
    }

    private void GetUserSelection(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (_waitingForInput)
        {
            Debug.Log("User confirmed input!");
            StopCoroutine(_trialRoutine);
            innerStimulus.SetActive(false);
            outerStimulus.SetActive(false);
            fixationDot.SetActive(false);

            var selectionLocation =
                outerStimulus.transform.InverseTransformPoint(laserManager.GetActiveSelectionTransform().position);

            _userInput = new InputData
            {
                ChosenDirection = angleSelectAction.axis.normalized,
                SelectionLocation = new Vector2(selectionLocation.x, selectionLocation.z)
            };
            _waitingForInput = false;
            _inputReceived = true;
            Session.instance.CurrentTrial.End();
        }
    }

    private (float, float)[] PartitionAperture()
    {
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
        var (start, end) = _apertureSlices[Random.Range(0, _apertureSlices.Length)];
        var randomAngle = Random.Range(start, end);

        var outerApertureRadius = Mathf.Tan(_outerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                             sessionSettings.stimulusDepth;
        var innerBuffer = Mathf.Tan(2 * _innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180.0f) *
                          sessionSettings.stimulusDepth;
        var randomRadialMagnitude = Random.Range(innerBuffer, outerApertureRadius);
        var randomPosition = Utility.Rotate2D(new Vector2(0.0f, randomRadialMagnitude), randomAngle);
        innerStimulus.transform.localPosition =
            new Vector3(randomPosition.x, randomPosition.y, sessionSettings.stimulusDepth);
        _innerStimulusSettings.correctAngle = Random.Range(0.0f, 360.0f);
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
            var angleError = Mathf.Acos(Vector2.Dot(_userInput.ChosenDirection, 
                Utility.Rotate2D(Vector2.up, _innerStimulusSettings.correctAngle))) * 180f / Mathf.PI;

            var innerStimulusPosition = innerStimulus.transform.localPosition;
            var positionError = (_userInput.SelectionLocation - new Vector2(innerStimulusPosition.x,
                innerStimulusPosition.y)).magnitude;
            trial.result["angle_error"] = angleError;
            trial.result["position_error"] = positionError;
        }
        else
        {
            trial.result["angle_error"] = "T/O";
            trial.result["position_error"] = "T/O";
        }
        
        if (_trialCount < sessionSettings.numTrials)
        {
            trial.block.CreateTrial();
            _trialCount++;
            Session.instance.NextTrial.Begin();
        }
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
