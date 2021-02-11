using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class ShowApertureSlicing : MonoBehaviour
{
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private Transform outerStimulusTransform;
    [SerializeField] private StimulusSettings outerStimulusSettings;

    private IEnumerable<Vector3> _lines;
    private bool _started;
    
    public void Start()
    {
        _started = true;
    }

    public void OnDrawGizmos()
    {
        if (!_started) return;
        _lines = GenerateLines();
        Gizmos.color = Color.red;
        foreach(var line in _lines)
        {
            Gizmos.DrawLine(outerStimulusTransform.localToWorldMatrix * new Vector4(0,0,0,1), line);
        }
    }
    
    private IEnumerable<Vector3> GenerateLines()
    {
        var apertureRadius = Mathf.Tan(outerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) *
                             outerStimulusSettings.stimDepthMeters;
        var upLine = new Vector2(0, apertureRadius);
        var lines = new Vector3[sessionSettings.regionSlices];
        var sliceSize = 360.0f / sessionSettings.regionSlices;
        
        for (var i = 0; i < sessionSettings.regionSlices; i++)
        {
            var rep2d = Utility.Rotate2D(upLine, i * sliceSize);
            lines[i] = outerStimulusTransform.localToWorldMatrix * new Vector4(rep2d.x, 0, rep2d.y, 1);
        }

        return lines;
    }
}
