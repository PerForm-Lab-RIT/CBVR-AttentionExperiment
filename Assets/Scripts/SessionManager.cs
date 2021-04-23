using PupilLabs;
using ScriptableObjects;
using Trial_Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    [SerializeField] private Text infoText;
    [SerializeField] private CalibrationController calibrationController;

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

    public void SetEnforcedHeadTransform()
    {
        trialManager.GetComponent<TrialManager>().SetEnforcedHeadTransform();
        infoText.text = "Head transform set!";
        infoText.color = Color.green;
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
            infoText.gameObject.SetActive(false);
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

        infoText.text = "";
        if (trialManagerComponent.IsPausable)
        {
            trialManagerComponent.Pause();
            pauseUI.SetActive(true);
            infoText.gameObject.SetActive(true);
            _isPaused = true;
        }
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        infoText.gameObject.SetActive(false);
        trialManager.GetComponent<TrialManager>().BeginTrial(Session.instance.CurrentTrial);
        _isPaused = false;
    }

    public void CalibratePupilLabs()
    {
        if (calibrationController.subsCtrl.IsConnected)
        {
            calibrationController.StartCalibration();
            calibrationController.OnCalibrationSucceeded += CalibrationSuccessful;
            calibrationController.OnCalibrationFailed += CalibrationFailed;
            experimenterUI.SetActive(false);
            pauseUI.SetActive(false);
            infoText.gameObject.SetActive(false);
        }
        else
        {
            infoText.text =
                "PupilLabs tracker disconnected!\n If the tracker was selected in UXF, ensure Pupil Capture is running and try again.";
            infoText.color = Color.red;
        }
    }

    private void CalibrationSuccessful()
    {
        if (_isPaused)
            pauseUI.SetActive(true);
        else
            experimenterUI.SetActive(true);
        infoText.gameObject.SetActive(true);
        infoText.text = "Calibration successful!";
        infoText.color = Color.green;
    }
    
    private void CalibrationFailed()
    {
        if (_isPaused)
            pauseUI.SetActive(true);
        else
            experimenterUI.SetActive(true);
        infoText.gameObject.SetActive(true);
        infoText.text = "Calibration failed.\n Ensure that Pupil Capture is running with both eye cameras!";
        infoText.color = Color.red;
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void ForceQuit()
    {
        Application.Quit();
    }
}