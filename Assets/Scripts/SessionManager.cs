using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UXF;

public class SessionManager : MonoBehaviour
{
    private int _winStreak;
    private int _score;
    private int _currentDifficulty;
    private Block _primaryBlock;

    [SerializeField] private SessionSettings settings;
    [SerializeField] private GameObject trialManager;

    public void StartSession(Session session)
    {
        settings.LoadFromUxfJson();
        
        SetSky(settings.skyColor);
        _primaryBlock = session.CreateBlock();
        var trial = _primaryBlock.CreateTrial();
        trial.settings.SetValue("difficulty", 0);
        
        trialManager.SetActive(true);
        trial.Begin();
    }
    
    private static void SetSky(Color skyColor)
    {
        RenderSettings.skybox.color = skyColor;
    }

    public void EndSession()
    {
        Application.Quit();
        EditorApplication.ExitPlaymode();
    }
}