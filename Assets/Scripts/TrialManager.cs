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


    public void Awake()
    {
        var fixationDotRadius = sessionSettings.fixationDotRadius * Mathf.PI / 180 * sessionSettings.stimulusDepth;
        fixationDot.transform.localScale = new Vector3(2.0f * fixationDotRadius, 0.0f, 2.0f * fixationDotRadius);
        fixationDot.transform.localPosition = new Vector3(0.0f, 0.0f, sessionSettings.stimulusDepth);
    }
    
    public void Update()
    {
        
    }

    public void BeginTrial(Trial trial)
    {
        StartCoroutine(nameof(TrialRoutine));
    }

    private IEnumerator TrialRoutine()
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
    }
}
