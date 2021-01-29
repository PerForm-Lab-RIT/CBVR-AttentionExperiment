using ScriptableObjects;
using UnityEngine;
using UXF;

// TODO
public class SessionManager : MonoBehaviour
{
    private int _winStreak;
    private int _score;
    private int _currentDifficulty;
    private Block _primaryBlock;
    private SessionSettings _settings;

    public void StartSession(Session session)
    {
        _settings = ScriptableObject.CreateInstance<SessionSettings>();
        SetSky();

        _primaryBlock = session.CreateBlock();
        var trial = _primaryBlock.CreateTrial();
        trial.settings.SetValue("difficulty", 0);
    }
    
    private void SetSky()
    {
        RenderSettings.skybox.color = Color.white;
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