using ScriptableObjects;
using UnityEngine;
using Trial_Manager;

public class HeadFixationBars : MonoBehaviour
{
    [SerializeField] private SessionSettings settings;
    [SerializeField] private GameObject trialManager;
    [SerializeField] private Transform cameraTransform;
    public TrialManager trialManagerScript;
    [SerializeField] private bool isTrueBar;
    
    void onEnable()
    {
        //transform.localPosition = Vector3.forward * settings.stimulusDepth;
        //transform.localRotation = Quaternion.Euler(0, 90, 0);
        trialManagerScript = trialManager.GetComponent<TrialManager>();
        Debug.Log("bar says head out of place");
    }

    private void Update()
    {
        if(isTrueBar)
        {
            transform.position = trialManagerScript.enforcedHeadPosition;// + (Vector3.forward * settings.stimulusDepth);
            //transform.rotation = trialManagerScript.enforcedHeadRotation * Quaternion.Euler(0, 90, 0);
            //transform.localRotation = Quaternion.Euler(0, 90, 0);


        }
        else
        {
        transform.localPosition = Vector3.forward * settings.stimulusDepth;
        transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

    }
}
