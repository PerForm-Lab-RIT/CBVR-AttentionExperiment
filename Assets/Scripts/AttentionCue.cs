using System;
using ScriptableObjects;
using UnityEngine;

public class AttentionCue : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float volume;
    [SerializeField] private AudioSource pulseSource;
    [SerializeField] private SessionSettings sessionSettings;

    [SerializeField] private float pulseFrequency;
    [SerializeField] private float sampleRate;
    [SerializeField] private float toneFrequency;

    private System.Random _random;
    
    private int _tick;

    public void Start()
    {
        _random = new System.Random();
    }

    public void OnEnable()
    {
        if (sessionSettings.sessionType == SessionSettings.SessionType.Training)
        {
            _tick = 0;
            pulseSource.Play();
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
            
            if (_tick >= (sampleRate * 1 / pulseFrequency))
                _tick = 0;
        }
    }
    
    private float CreatePulse()
    {
        var rand = _random.NextDouble();
        return _tick >= (sampleRate * 1 / (2 * pulseFrequency)) ? 0f : (float) rand;
    }
}
