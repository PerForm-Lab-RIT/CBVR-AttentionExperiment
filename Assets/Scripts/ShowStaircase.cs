using System;
using System.Globalization;
using ScriptableObjects;
using TMPro;
using Trial_Manager;
using UnityEngine;

public class ShowStaircase : MonoBehaviour
{
    [SerializeField] private TrialManager trialManager;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float textSize;
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private float offsetInDegrees;
    
    private void OnEnable()
    {
        text.fontSize = textSize * sessionSettings.stimulusDepth;
        var offset = Mathf.Tan(offsetInDegrees * Mathf.PI / 180f)
                     * sessionSettings.stimulusDepth;
        text.transform.localPosition = new Vector3(offset, 0, -offset);
        text.gameObject.SetActive(true);
    }

    public void UpdateStaircaseDisplay()
    {
        text.text = 
            Convert.ToString(trialManager.StaircaseManager.CurrentStaircase.CurrentStaircaseAngle(), CultureInfo.InvariantCulture);
    }
}
