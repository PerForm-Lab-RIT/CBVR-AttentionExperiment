using System.Collections;
using DotStimulus;
using ScriptableObjects;
using UnityEngine;
using UXF;

public class TrialManager : MonoBehaviour
{
    [SerializeField] private GameObject outerStimulus;
    [SerializeField] private GameObject innerStimulus;
    [SerializeField] private GameObject fixationDot;
    [SerializeField] private SessionSettings sessionSettings;

    private int _trialCount = 1;
    private StimulusSettings _innerStimulusSettings;
    private StimulusSettings _outerStimulusSettings;
    private (float, float)[] _apertureSlices;

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
        _innerStimulusSettings.correctAngle = Random.Range(0, 360);
        innerStimulus.GetComponent<DotManager>().InitializeWithSettings(_innerStimulusSettings);
        StartCoroutine(TrialRoutine(trial));
    }

    public void EndTrial(Trial trial)
    {
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
        innerStimulus.SetActive(false);
        yield return new WaitForSeconds((sessionSettings.outerStimulusDuration - sessionSettings.innerStimulusDuration) / 1000);
        outerStimulus.SetActive(false);
        trial.End();
    }
}
