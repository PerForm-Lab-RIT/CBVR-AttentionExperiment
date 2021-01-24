using ScriptableObjects;
using UnityEngine;

public class Dot
{
    private readonly Vector2 _velocity;
    private Vector3 _position;
    private Vector3 _oldPosition;
    private readonly float _scale;
    private StimulusSettings _settings;

    private float _apertureRadius;
    private readonly float _sqrApertureRadius;
    private float _elapsedTime;
    
    public Dot(Vector2 velocity, Vector3 startingPosition, StimulusSettings settings)
    {
        _velocity = velocity;
        _position = startingPosition;
        _settings = settings;
        _elapsedTime = 0;
        
        // In case first step puts dot outside circle
        _oldPosition = new Vector3(velocity.x, 0 ,velocity.y);

        var dotSize = settings.dotSizeArcMinutes * Mathf.PI / (60 * 180) * settings.stimDepthMeters;
        _apertureRadius = Mathf.Tan(settings.apertureRadiusDegrees * Mathf.PI / 180) * settings.stimDepthMeters;
        _sqrApertureRadius = _apertureRadius * _apertureRadius;
        _scale = dotSize;
    }

    public void UpdateDot()
    {
        if (_elapsedTime > _settings.dotLifetime)
        {
            var randomPosition = Random.insideUnitCircle * (_apertureRadius - 0.1f);
            _position.x = randomPosition.x;
            _position.z = randomPosition.y;
            _elapsedTime = 0.0f;
        }
        else
        {
            _position.x += _velocity.x * Time.deltaTime;
            _position.z += _velocity.y * Time.deltaTime;

            // Square magnitude used for efficiency
            if (_position.sqrMagnitude > _sqrApertureRadius)
            {
                _position.x = -_oldPosition.x;
                _position.z = -_oldPosition.z;
            }
        }

        _elapsedTime += Time.deltaTime;
        _oldPosition = _position;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }
    
    public float GetScale()
    {
        return _scale;
    }
}
