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
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        text.fontSize = textSize * sessionSettings.stimulusDepth;
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void UpdateStaircaseDisplay()
    {
        text.text = 
            Convert.ToString(trialManager.StaircaseManager.CurrentStaircase.CurrentStaircaseLevel(), CultureInfo.InvariantCulture);
    }
}
