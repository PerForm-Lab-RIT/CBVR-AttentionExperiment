using System;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

public class DotManager : MonoBehaviour
{
    [SerializeField] private bool buddyDotNoiseGeneration;
    [SerializeField] private Mesh dotMesh;
    [SerializeField] private float boundsRange;
    [SerializeField] private Transform viewOrigin;
    private Bounds _bounds;
    
    [SerializeField] private Material dotMeshMaterial;
    [SerializeField] private StimulusSettings stimulusSettings;

    private Dot[] _dots;

    [SerializeField] 
    private int numDots;
    
    private MeshProperties[] _meshProperties;
    private ComputeBuffer _meshPropertiesBuffer;

    private uint[] _args;
    private ComputeBuffer _argsBuffer;
    private static readonly int Properties = Shader.PropertyToID("_Properties");

    public const float ApertureTolerance = 0.001f;

    public void Start()
    {
        // Copying the material into a new instance allows for multiple active stimuli
        // at the same time as they require separate material buffers
        dotMeshMaterial = new Material(dotMeshMaterial);
        
        numDots = Mathf.RoundToInt(Mathf.Pow(stimulusSettings.apertureRadiusDegrees, 2.0f) * Mathf.PI *
                                   stimulusSettings.density);
        if (numDots % 2 != 0)
            numDots += 1;

        // Some setup for custom shader
        InitializeBuffers();
        _bounds = new Bounds(transform.position, Vector3.one * (boundsRange + 1));

        var apertureRadius = Mathf.Tan(stimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) * stimulusSettings.stimDepthMeters;
        var dotSpeed = stimulusSettings.speed * ((Mathf.PI) / 180) * stimulusSettings.stimDepthMeters;

        if (buddyDotNoiseGeneration)
            GenerateDotsWithBuddy(apertureRadius, dotSpeed);
        else
            GenerateDots(apertureRadius, dotSpeed);
    }

    public void Update()
    {
        for(var i = 0; i < _dots.Length; i++)
        {
            _dots[i].UpdateDot();

            _meshProperties[i].LocalPosition = _dots[i].GetPosition();
            _meshProperties[i].LocalScale = _dots[i].GetScale();
            _meshProperties[i].ParentLocalToWorld = viewOrigin.transform.localToWorldMatrix;
        }
        
        _meshPropertiesBuffer.SetData(_meshProperties);
        dotMeshMaterial.SetBuffer(Properties, _meshPropertiesBuffer);
        Graphics.DrawMeshInstancedIndirect(dotMesh, 0, dotMeshMaterial, _bounds, _argsBuffer);
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
            var randomPosition = Random.insideUnitCircle * (apertureRad - ApertureTolerance);
            var randomVelocity = Random.insideUnitCircle.normalized * speed;
            _dots[i] = new Dot(randomVelocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                stimulusSettings);
        }
        for (; i < numDots; i++)
        {
            var randomPosition = Random.insideUnitCircle * (apertureRad - ApertureTolerance);
            var velocity = Rotate(Vector2.up, stimulusSettings.coherenceAngle) * speed;
            
            _dots[i] = new Dot(velocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                stimulusSettings);
        }
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
            var randomPosition = Random.insideUnitCircle * (apertureRad - ApertureTolerance);
            var randomBuddyPosition = Random.insideUnitCircle * (apertureRad - ApertureTolerance);
            
            var randomVelocity = Random.insideUnitCircle.normalized * speed;
            var buddyVelocity = new Vector2(-randomVelocity.x, -randomVelocity.y);
            _dots[i] = new Dot(randomVelocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                stimulusSettings);
            _dots[i+1] = new Dot(buddyVelocity, new Vector3(randomBuddyPosition.x, 0, randomBuddyPosition.y),
                stimulusSettings);
        }
        for (; i < numDots; i++)
        {
            var randomPosition = Random.insideUnitCircle * (apertureRad - ApertureTolerance);
            var velocity = Rotate(Vector2.up, stimulusSettings.coherenceAngle) * speed;
            
            _dots[i] = new Dot(velocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                stimulusSettings);
        }
    }
    
    private static Vector2 Rotate(Vector2 v, float deltaDegrees)
    {
        var angleRadians = deltaDegrees * (float) Math.PI / 180.0f;
        return new Vector2(
            v.x * Mathf.Cos(angleRadians) - v.y * Mathf.Sin(angleRadians),
            v.x * Mathf.Sin(angleRadians) + v.y * Mathf.Cos(angleRadians)
        );
    }
    
    private void InitializeBuffers()
    {
        _meshProperties = new MeshProperties[numDots];
        _meshPropertiesBuffer = new ComputeBuffer(numDots, MeshProperties.Size());

        _args = new uint[5]
        {
            dotMesh.GetIndexCount(0),
            (uint) numDots,
            dotMesh.GetIndexStart(0),
            dotMesh.GetBaseVertex(0),
            0
        };
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);
    }

    // Data sent to shader
    private struct MeshProperties {
        public Vector3 LocalPosition;
        public float LocalScale;
        public Matrix4x4 ParentLocalToWorld;

        public static int Size()
        {
            return (sizeof(float) * 4 * 4) +
                   (sizeof(float) * 4);
        }
    }

    public void OnDestroy()
    {
        _meshPropertiesBuffer?.Release();
        _meshPropertiesBuffer = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }
}
