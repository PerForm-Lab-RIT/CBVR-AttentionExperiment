using ScriptableObjects;
using UnityEngine;

namespace DotStimulus
{
    public class Dot
    {
        private readonly Vector2 _velocity;
        public Vector3 Position { get; private set; }
        private Vector3 _currentPosition;
        private Vector3 _oldPosition;
        public readonly float scale;
        private readonly StimulusSettings _settings;

        private readonly float _apertureRadius;
        private readonly float _sqrApertureRadius;
        private float _elapsedTime;

        public Dot(Vector2 velocity, Vector3 startingPosition, StimulusSettings settings)
        {
            _velocity = velocity;
            _currentPosition = startingPosition;
            Position = _currentPosition;
            _settings = settings;
            _elapsedTime = 0;

            var dotSize = settings.dotSizeArcMinutes * Mathf.PI / (60 * 180) * settings.stimDepthMeters;
            _apertureRadius = Mathf.Tan(settings.apertureRadiusDegrees * Mathf.PI / 180) * settings.stimDepthMeters;
            _sqrApertureRadius = _apertureRadius * _apertureRadius;
            scale = dotSize;
        
            // In case first update puts dot outside circle
            var randomPosition = Random.insideUnitCircle * _apertureRadius;
            _oldPosition = new Vector3(randomPosition.x, 0, randomPosition.y);
        }

        public void UpdateDot()
        {
            var deltaTime = Time.deltaTime;
            
            if (_elapsedTime > Random.Range(_settings.minDotLifetime, _settings.maxDotLifetime))
            {
                var randomPosition = Random.insideUnitCircle * _apertureRadius;
                _currentPosition.x = randomPosition.x;
                _currentPosition.z = randomPosition.y;
                _elapsedTime = 0.0f;
            }
            else
            {
                _currentPosition.x += _velocity.x * deltaTime;
                _currentPosition.z += _velocity.y * deltaTime;

                // Square magnitude used for efficiency
                if (_currentPosition.sqrMagnitude > _sqrApertureRadius)
                {
                    _currentPosition.x = -_oldPosition.x;
                    _currentPosition.z = -_oldPosition.z;
                }
            }

            Position = _currentPosition;
            _elapsedTime += deltaTime;
            _oldPosition = _currentPosition;
        }
    }
}
