using System.Collections;
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
    private bool isTrialFinished;
    
    public void Start()
    {
        if(sessionSettings.regionSlices % 2 != 0)
            Debug.LogWarning("Odd number of aperture slices detected! Please use an even number.");
        
        var apertureSlices = PartitionAperture();

        var fixationDotRadius = sessionSettings.fixationDotRadius * Mathf.PI / 180 * sessionSettings.stimulusDepth;
        fixationDot.transform.localScale = new Vector3(2.0f * fixationDotRadius, 0.0f, 2.0f * fixationDotRadius);
        fixationDot.transform.localPosition = new Vector3(0.0f, 0.0f, sessionSettings.stimulusDepth);
    }

    private (float, float)[] PartitionAperture()
    {
        var slices = new (float, float)[sessionSettings.regionSlices];
        var sliceSize = 360.0f / sessionSettings.regionSlices;

        for (var i = (sessionSettings.flipRegions) ? 1 : 0; i < _apertureSlices.Length; i += 2)
        {
            slices[i] = (i * sliceSize, (i + 1) * sliceSize);
        }

        return slices;
    }

    public void BeginTrial(Trial trial)
    {
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
