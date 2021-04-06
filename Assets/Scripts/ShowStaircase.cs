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
    
    private void OnEnable()
    {
        text.fontSize = textSize * sessionSettings.stimulusDepth;
        text.transform.localPosition = new Vector3(0, 0, 0);
        text.gameObject.SetActive(true);
    }

    public void UpdateStaircaseDisplay()
    {
        text.text = 
            Convert.ToString(trialManager.StaircaseManager.CurrentStaircase.CurrentStaircaseLevel(), CultureInfo.InvariantCulture);
    }
}
