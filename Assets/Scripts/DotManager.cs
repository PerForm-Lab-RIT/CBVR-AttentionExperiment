using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

public class DotManager : MonoBehaviour
{
    [SerializeField] private Mesh dotMesh;
    [SerializeField] private float boundsRange;
    [SerializeField] private Transform viewOrigin;
    private Bounds _bounds;
    
    [SerializeField] private Material dotMeshMaterial;
    [SerializeField] private StimulusSettings stimulusSettings;

    private Dot[] _dots;

    public int numDots;
    
    private MeshProperties[] _meshProperties;
    private ComputeBuffer _meshPropertiesBuffer;

    private uint[] _args;
    private ComputeBuffer _argsBuffer;

    public void Start()
    {
        numDots = Mathf.RoundToInt(Mathf.Pow(stimulusSettings.apertureRadiusDegrees, 2.0f) * Mathf.PI *
                                   stimulusSettings.density);
        _dots = new Dot[numDots];
        
        // Some setup for custom shader
        InitializeBuffers(numDots);
        _bounds = new Bounds(transform.position, Vector3.one * (boundsRange + 1));

        var apertureRadius = Mathf.Tan(stimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) * stimulusSettings.stimDepthMeters;
        var dotSpeed = stimulusSettings.speed * ((Mathf.PI) / 180) * stimulusSettings.stimDepthMeters;
        for (var i = 0; i < _dots.Length; i++)
        {
            var randomUnit = Random.insideUnitCircle;
            
            // 0.1 is subtracted to absolutely ensure the dot is spawned inside the aperture.
            // This prevents a 'ring effect' because of dots being spawned in the center
            var randomPosition = randomUnit * (apertureRadius - 0.1f);
            
            var randomVelocity = Random.insideUnitCircle.normalized * dotSpeed;
            _dots[i] = new Dot(randomVelocity, new Vector3(randomPosition.x, 0, randomPosition.y),
                stimulusSettings);
        }
    }
    
    public void Update()
    {
        for(var i = 0; i < _dots.Length; i++)
        {
            _dots[i].UpdateDot();

            _meshProperties[i].localPosition = _dots[i].GetPosition();
            _meshProperties[i].localScale = _dots[i].GetScale();
            _meshProperties[i].parentLocalToWorld = viewOrigin.transform.localToWorldMatrix;
            
        }
        _meshPropertiesBuffer.SetData(_meshProperties);
        dotMeshMaterial.SetBuffer("_Properties", _meshPropertiesBuffer);
        Graphics.DrawMeshInstancedIndirect(dotMesh, 0, dotMeshMaterial, _bounds, _argsBuffer);
    }
    
    private void InitializeBuffers(int numDots)
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

    private struct MeshProperties {
        public Vector3 localPosition;
        public float localScale;
        public Matrix4x4 parentLocalToWorld;

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
