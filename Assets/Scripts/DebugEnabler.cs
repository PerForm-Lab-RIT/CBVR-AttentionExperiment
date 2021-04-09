using ScriptableObjects;
using UnityEngine;

public class DebugEnabler : MonoBehaviour
{
    [SerializeField] private DebugSettings settings;
    [SerializeField] private GameObject gazeDebug;
    [SerializeField] private GameObject staircaseDebug;
    
    public void EnableFromSettings()
    {
        settings.LoadFromUxfJson();
        if(settings.showGaze)
            gazeDebug.SetActive(true);
        if(settings.showStaircase)
            staircaseDebug.SetActive(true);
    }
}
