using System;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DotStimulus
{
    public class DotManager : MonoBehaviour
    {
        [SerializeField] private bool buddyDotNoiseGeneration;
        [SerializeField] private Mesh dotMesh;
        [SerializeField] private Material dotMeshMaterial;
        [SerializeField] private StimulusSettings stimulusSettings;
        [SerializeField] private int numDots;
        [SerializeField] private float randomDepthRange;
        
        private Dot[] _dots;
        private DotShaderData _shaderData;
        
        private Bounds _bounds;
        private const float BoundsRange = 5.0f;
        private static readonly int ShaderProperties = Shader.PropertyToID("_Properties");
        private static readonly int ParentLocalToWorld = Shader.PropertyToID("parentLocalToWorld");

        public void Awake()
        {
            InitializeWithSettings(stimulusSettings);
            
            // Copying the material into a new instance allows for multiple active stimuli
            // at the same time as they require separate material buffers
            dotMeshMaterial = new Material(dotMeshMaterial);
            _bounds = new Bounds(transform.position, Vector3.one * (BoundsRange + 1));
        }

        public void Start()
        {
            var worldTransform = transform;
            var localPosition = worldTransform.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y, stimulusSettings.stimDepthMeters);
            worldTransform.localPosition = localPosition;
        }
        
        public void Update()
        {
            for (var i = 0; i < _dots.Length; i++)
            {
                _dots[i].UpdateDot();
                _shaderData.UpdateMeshPropertiesBuffer(_dots, i);
            }

            var meshPropertiesBuffer = _shaderData.MeshPropertiesBuffer;
            meshPropertiesBuffer.SetData(_shaderData.MeshProps);
            dotMeshMaterial.SetBuffer(ShaderProperties, meshPropertiesBuffer);
            dotMeshMaterial.SetMatrix(ParentLocalToWorld, transform.localToWorldMatrix);
            Graphics.DrawMeshInstancedIndirect(dotMesh, 0, dotMeshMaterial, _bounds, _shaderData.ArgsBuffer);
        }
        
        public void InitializeWithSettings(StimulusSettings settings)
        {
            stimulusSettings = settings;
            var apertureRadius = Mathf.Tan(stimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) *
                                 stimulusSettings.stimDepthMeters;

            var approxMeterPerDegree = Mathf.Tan(1.0f * Mathf.PI / 180.0f) * stimulusSettings.stimDepthMeters;
            numDots = Mathf.RoundToInt(Mathf.Pow( apertureRadius / approxMeterPerDegree, 2.0f) * Mathf.PI *
                                       stimulusSettings.density);
            
            if (buddyDotNoiseGeneration && numDots % 2 != 0)
                numDots += 1;
            
            _shaderData?.ClearBuffers();
            _shaderData = new DotShaderData(dotMesh, numDots);
            
            var dotSpeed = stimulusSettings.speed * (Mathf.PI / 180) * stimulusSettings.stimDepthMeters;

            if (buddyDotNoiseGeneration)
                GenerateDotsWithBuddy(apertureRadius, dotSpeed);
            else
                GenerateDots(apertureRadius, dotSpeed);
        }

        public StimulusSettings GetSettings()
        {
            return stimulusSettings;
        }
    
        private void GenerateDots(float apertureRad, float speed)
        {
            _dots = new Dot[numDots];
            var numNoiseDots = (int) (numDots * (stimulusSettings.noiseDotPercentage / 100.0f));

            int i;
            for (i = 0; i < numNoiseDots; i++)
            {
                // 0.1 is subtracted to absolutely ensure the dot is spawned inside the aperture.
                // This prevents a 'ring effect' because of dots being spawned in the center
                var randomPosition = Random.insideUnitCircle * apertureRad;
                var randomVelocity = Random.insideUnitCircle.normalized * speed;
                var randomDepth = 2 * (Random.value - 0.5f) * randomDepthRange;
                    
                _dots[i] = new Dot(randomVelocity, new Vector3(randomPosition.x, randomDepth, randomPosition.y),
                    stimulusSettings);
            }
            GenerateRemainingSignalDots(apertureRad, speed, i);
        }

        private void GenerateDotsWithBuddy(float apertureRad, float speed)
        {
            if (numDots % 2 != 0)
                numDots += 1;
            _dots = new Dot[numDots];
            var numNoiseDots = (int) (numDots * (stimulusSettings.noiseDotPercentage / 100.0f));

            int i;
            for (i = 0; i < numNoiseDots; i += 2)
            {
                // 0.1 is subtracted to absolutely ensure the dot is spawned inside the aperture.
                // This prevents a 'ring effect' because of dots being spawned in the center
                var randomPosition = Random.insideUnitCircle * apertureRad;
                var randomBuddyPosition = Random.insideUnitCircle * apertureRad;
            
                var randomVelocity = Random.insideUnitCircle.normalized * speed;
                var buddyVelocity = new Vector2(-randomVelocity.x, -randomVelocity.y);
                _dots[i] = new Dot(randomVelocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                    stimulusSettings);
                _dots[i+1] = new Dot(buddyVelocity, new Vector3(randomBuddyPosition.x, 0, randomBuddyPosition.y),
                    stimulusSettings);
            }
            GenerateRemainingSignalDots(apertureRad, speed, i);
        }
        
        private void GenerateRemainingSignalDots(float apertureRad, float speed, int i)
        {
            for (; i < numDots; i++)
            {
                var randomPosition = Random.insideUnitCircle * apertureRad;
                var velocityAngle = stimulusSettings.correctAngle
                                    + Random.Range(-stimulusSettings.coherenceRange / 2,
                                        stimulusSettings.coherenceRange / 2);

                var velocity = Utility.Rotate2D(Vector2.up, velocityAngle) * speed;

                _dots[i] = new Dot(velocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                    stimulusSettings);
            }
        }

        public void OnDestroy()
        {
            _shaderData.ClearBuffers();
        }

        public void OnApplicationQuit()
        {
            _shaderData.ClearBuffers();
        }
    }
}
