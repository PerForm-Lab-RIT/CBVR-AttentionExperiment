using ScriptableObjects;
using TMPro;
using UnityEngine;

public class StartText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SessionSettings sessionSettings;

    public void OnEnable()
    {
        text.fontSize = sessionSettings.stimulusDepth;
        gameObject.transform.localPosition = new Vector3(0, 0, sessionSettings.stimulusDepth);
    }
}
