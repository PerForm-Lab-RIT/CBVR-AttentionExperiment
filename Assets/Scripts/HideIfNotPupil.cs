using System;
using ScriptableObjects;
using UnityEngine;
using UXF;

public class HideIfNotPupil : MonoBehaviour
{
    [SerializeField] private SessionSettings settings;
    
    public void OnEnable()
    {
        var sessionSettingsDict = Session.instance.settings.GetDict("SessionSettings");
        var tracker = (string) sessionSettingsDict["EyeTracker"];
        if(char.ToLower(tracker[0]) != 'p')
        {
            gameObject.SetActive(false);
        }
    }
}
