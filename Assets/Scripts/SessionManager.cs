using ScriptableObjects;
using UnityEngine;
using UXF;

public class SessionManager : MonoBehaviour
{
    private int _winStreak;
    private int _score;
    private int _currentDifficulty;
    private Block _primaryBlock;
    
    [SerializeField] private SessionSettings settings;
    [SerializeField] private TrialManager trialManager;

    public void StartSession(Session session)
    {
        settings.LoadFromUxfJson();
        SetSky(settings.skyColor);

        _primaryBlock = session.CreateBlock();
        var trial = _primaryBlock.CreateTrial();
        trial.settings.SetValue("difficulty", 0);
        trial.Begin();
    }
    
    private static void SetSky(Color skyColor)
    {
        RenderSettings.skybox.color = skyColor;
    }

    public void RunTrial(Trial trial)
    {
        // TODO: Generate a stimulus
        // Get input, or timeout
        // Generate response
        // Gather data
        // Generate info/params for next trial(s)
    }
}