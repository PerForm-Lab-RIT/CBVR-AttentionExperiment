using System;
using ScriptableObjects;
using UnityEngine;

public class AttentionCue : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float volume;
    [SerializeField] private AudioSource pulseSource;
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private StimulusSettings innerStimulusSettings;
    [SerializeField] private GameObject innerStimulus;

    private float _pulseFrequency;
    private float _sampleRate;

    private System.Random _random;
    
    private int _tick;
    private Vector2 _velocity;

    public void Start()
    {
        _random = new System.Random();
        _pulseFrequency = sessionSettings.pulseFrequency;
        _sampleRate = sessionSettings.sampleRate;
    }

    public void OnEnable()
    {
        // Cue is disabled in testing mode
        if (sessionSettings.sessionType == SessionSettings.SessionType.Testing) return;
        
        switch (sessionSettings.cueType)
        {
            case SessionSettings.CueType.Neutral:
                gameObject.transform.localPosition = new Vector3(0, 0, sessionSettings.attentionCueDepth);
                break;
            
            case SessionSettings.CueType.FeatureBased:
                var direction = Utility.Rotate2D(Vector2.up, innerStimulusSettings.correctAngle);
                var cueDistance = Mathf.Tan(sessionSettings.attentionCueDistance * Mathf.PI / 180f) * sessionSettings.attentionCueDepth;
                var startingPosition = -cueDistance * direction;
                gameObject.transform.localPosition = new Vector3(startingPosition.x, startingPosition.y, sessionSettings.attentionCueDepth);

                var speed = cueDistance / sessionSettings.attentionCueDuration;
                _velocity = speed * direction;
                break;

            case SessionSettings.CueType.StimulusBased:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _tick = 0;
        pulseSource.Play();
    }

    public void Update()
    {
        if (sessionSettings.cueType == SessionSettings.CueType.FeatureBased)
        {
            var selfObject = gameObject;
            var position = selfObject.transform.localPosition;
            var delta = _velocity * (2 * Time.deltaTime);
            selfObject.transform.localPosition = new Vector3(position.x + delta.x, position.y + delta.y, sessionSettings.attentionCueDepth);
        // The object cannot be parented to the inner stimulus since it is disabled when the cue should play
        } else if (sessionSettings.cueType == SessionSettings.CueType.StimulusBased)
        {
            gameObject.transform.localPosition = innerStimulus.transform.localPosition;
        }
    }

    public void OnDisable()
    {
        if(pulseSource.isPlaying)
            pulseSource.Stop();
    }

    public void OnAudioFilterRead(float[] data, int channels)
    {
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = volume * CreatePulse();

            if (channels == 2)
            {
                data[i + 1] = data[i];
                i++;
            }

            _tick++;
            
            if (_tick >= (_sampleRate * 1 / _pulseFrequency))
                _tick = 0;
        }
    }
    
    private float CreatePulse()
    {
        var rand = _random.NextDouble();
        return _tick >= (_sampleRate * 1 / (2 * _pulseFrequency)) ? 0f : (float) rand;
    }
}
