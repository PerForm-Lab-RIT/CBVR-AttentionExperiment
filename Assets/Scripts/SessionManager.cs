using System;
using ScriptableObjects;
using Trial_Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;
using Valve.VR;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private SessionSettings settings;
    
    // Variable: trialManager
    // A GameObject containing a TrialManager monobehaviour. Should start inactive.
    [SerializeField] private GameObject trialManager;
    [SerializeField] private SteamVR_Action_Boolean confirmInputAction;
    [SerializeField] private GameObject experimenterUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject spectatorUI;
    [SerializeField] private GameObject startText;

    private Session _session;
    private Block _primaryBlock;
    private bool _experimenterStartControlsEnabled;
    private bool _experimenterPauseControlsEnabled;
    private bool _sessionStarted;
    private bool _sessionOver;

    private bool _isPaused;
    
    public void OnEnable()
    {
        confirmInputAction.onStateDown += StartFirstTrial;
        confirmInputAction.onStateDown += CloseProgram;
    }

    public void OnDisable()
    {
        confirmInputAction.onStateDown -= StartFirstTrial;
        confirmInputAction.onStateDown -= CloseProgram;
    }
    
    // Called via UXF Event
    public void StartSession(Session session)
    {
        settings.LoadFromUxfJson();
        StartExperimenterView();
        SetSky(settings.skyColor);
        _session = session;
    }

    private void StartExperimenterView()
    {
        experimenterUI.SetActive(true);
        _experimenterStartControlsEnabled = true;
    }

    public void StartMainSession()
    {
        if (_experimenterStartControlsEnabled)
        {
            _primaryBlock = _session.CreateBlock();
            _primaryBlock.CreateTrial();
            
            _experimenterStartControlsEnabled = false;
            trialManager.SetActive(true);
            spectatorUI.SetActive(true);
            experimenterUI.SetActive(false);
            startText.SetActive(true);
            _sessionStarted = true;
        }
    }

    private static void SetSky(Color skyColor)
    {
        RenderSettings.skybox.color = skyColor;
    }

    private void StartFirstTrial(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (_sessionStarted && trialManager.activeInHierarchy)
        {
            Session.instance.BeginNextTrial();
            _sessionStarted = false;
        }
    }
    
    public void StartFirstTrial(InputAction.CallbackContext context)
    {
        if (_sessionStarted && context.started)
        {
            Session.instance.BeginNextTrial();
            _sessionStarted = false;
        }
    }

    // Called via UXF Event
    public void EndSession()
    {
        _sessionOver = true;
    }

    private void CloseProgram(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if(_sessionOver)
            Application.Quit();
    }
    
    public void CloseProgram()
    {
        if(_sessionOver)
            Application.Quit();
    }

    public void Pause()
    {
        var trialManagerComponent = trialManager.GetComponent<TrialManager>();

        if (trialManagerComponent.IsPausable)
        {
            trialManagerComponent.Pause();
            pauseUI.SetActive(true);
            _isPaused = true;
        }
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        trialManager.GetComponent<TrialManager>().BeginTrial(Session.instance.CurrentTrial);
        _isPaused = false;
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void ForceQuit()
    {
        Application.Quit();
    }
}