using System;
using System.Collections;

using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using ScriptableObjects.Variables;
using UnityEngine;
using UXF;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

[ExecuteInEditMode]
public class CalibrationAssessment : MonoBehaviour
{
    public float fixationTime => 1.0f;

    public float targetRadiusInDegrees;
    public float targetDistance = 1;
    
    public float azimuthWidth = 10.0f;
    public float elevationHeight = 15.0f;

    public bool randomizeTargetOrder;

    [NonSerialized] public bool IsHeadFixed = true;

    private static readonly System.Random Random = new System.Random();
    
    public Transform eyeInHeadTransform;
    [NonSerialized] public Transform CurrentTargetTransform;

    private bool _presentingTarget;
    public float gazeToTargetDist;

    private readonly List<int> _remainingTargets = new List<int>() {0,1,2,3,4,5,6,7,8};
    private int _targetIdx;

    public Text gazeErrorText;
    
    private MeshRenderer[] _targetRenderers;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    [SerializeField] private SelectEyeTracker eyeTracker;
    [SerializeField] private Camera vrCamera;
    [SerializeField] private GameObject[] targets;
    [SerializeField] private Tracker assessmentTracker;
    [SerializeField] private SessionSettings settings;
    [SerializeField] private IntVariable trialCount;
    private GameObject[][] _targets2D;

    public void OnValidate(){ 
        Debug.Log("Adjusting calibration targets.");
    }

    public void OnEnable()
    {
        _targets2D = new GameObject[3][];
        _targets2D[0] = new [] {targets[0], targets[1], targets[2]};
        _targets2D[1] = new [] {targets[3], targets[4], targets[5]}; 
        _targets2D[2] = new [] {targets[6], targets[7], targets[8]};
        _targetRenderers = new MeshRenderer[targets.Length];
        for (var i = 0; i < targets.Length; i++)
        {
            _targetRenderers[i] = targets[i].GetComponent<MeshRenderer>();
            _targetRenderers[i].enabled = false;
        }
        _targetRenderers[0].enabled = true;
        _targetIdx = 0;
        targetDistance = settings.stimulusDepth;
        CurrentTargetTransform = GETTargTransformByIndex(0);
        
        RepositionTargets();

        if(randomizeTargetOrder){
            ShuffleTargetList();
        }
    }

    private void RepositionTargets(){
        var halfAzRad = (azimuthWidth/2.0f) * Mathf.Deg2Rad;
        var halfElRad = (elevationHeight/2.0f) * Mathf.Deg2Rad;

        for(var c = 1; c < 4; ++c) {
            for(var r = 1; r < 4; ++r) {
                var targetTrans = _targets2D[c-1][r-1].transform;
                
                var longitude = halfElRad - halfElRad * (r-1);
                var latitude = -halfAzRad + halfAzRad * (c-1);

                var a = targetDistance * Mathf.Cos(longitude);
                var zPos = a * Mathf.Cos(latitude);
                var yPos = targetDistance * Mathf.Sin(longitude);
                var xPos = a * Mathf.Sin(latitude);
                
                var newTargetPos = new Vector3(xPos,yPos,zPos);

                targetTrans.localPosition = newTargetPos;

                var newScale = 2 * Mathf.Tan(targetRadiusInDegrees  * ( Mathf.PI / 180) * targetDistance);
                targetTrans.localScale = new Vector3(newScale,newScale,newScale);
            }
        }
                
        var targetBacking = transform.Find("backing");
        targetBacking.GetComponent<MeshFilter>().mesh.triangles = 
            targetBacking.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        targetBacking.gameObject.AddComponent<MeshCollider>();
        targetBacking.localScale = new Vector3(targetDistance*2.0f, targetDistance*2.0f, targetDistance*2.0f);
    }

    private void ShuffleTargetList(){
        var n = _remainingTargets.Count;  
        while (n > 1) 
        {  
            n--;  
            var k = Random.Next(n + 1);  
            var value = _remainingTargets[k];  
            _remainingTargets[k] = _remainingTargets[n];  
            _remainingTargets[n] = value;  
        }
    }

    private void NextTarget() {
        ToggleTargetByIndex(_targetIdx); // turn current target off
        
        _targetIdx +=1;

        if( _targetIdx > 8){
            _targetIdx = 0;
        }
        
        ToggleTargetByIndex(_targetIdx); // turn next target on
    }

    private void PreviousTarget() {

        ToggleTargetByIndex(_targetIdx); // turn current target off
        
        _targetIdx -= 1;
        
        if( _targetIdx < 0){
            _targetIdx = 8;
        }

        ToggleTargetByIndex(_targetIdx); // turn next target on
    }

    private Transform GETTargTransformByIndex(int targetIdx){
        
        var targetNum = _remainingTargets[targetIdx];
        
        var rowCol = GETTargetRowCol(targetNum);

        var targetName = "gazeTarget_r"+ rowCol[0].ToString() + "c" + rowCol[1].ToString();
        var targetTrans = transform.Find(targetName);

        return targetTrans;
    }


    private void RecordFixation(){
        var mr = CurrentTargetTransform.gameObject.GetComponent<MeshRenderer>();

        StartCoroutine(PresentTarget());

        IEnumerator PresentTarget()
        {
            
            mr.material.SetColor(ColorProperty, Color.yellow);
            _presentingTarget = true;

            
            assessmentTracker.StartRecording();
            yield return new WaitForSeconds(fixationTime);
            assessmentTracker.StopRecording();
            
            mr.material.SetColor(ColorProperty, Color.black);
            Session.instance.SaveDataTable(assessmentTracker.data, "EyeTrackerAssessmentTrial" + trialCount.value);
            Debug.Log("Data recorded for target");
            _presentingTarget = false;
        }
    }

    private void ToggleTargetByIndex(int targetIdx)
    {
        
        CurrentTargetTransform = GETTargTransformByIndex(targetIdx);
        var mr = CurrentTargetTransform.gameObject.GetComponent<MeshRenderer>();
        var sphereCollider = CurrentTargetTransform.gameObject.GetComponent<SphereCollider>();
        
        if( mr.enabled )
        {
            if( _presentingTarget ){
                mr.material.SetColor(ColorProperty, Color.black);      
            }

            mr.enabled = false;
            sphereCollider.enabled = false;
        } else
        {
            mr.enabled = true;
            sphereCollider.enabled = true;

            if( _presentingTarget ) {
                mr.material.SetColor(ColorProperty, Color.yellow);
            }
        }
    }

    private static List<int> GETTargetRowCol(int targNum){
        var r = 1 + Math.Floor( (double)targNum / 3);
        var c = 1 + (targNum%3);
        return new List<int>{(int)r,c};
    }

    private void ToggleHeadFixed(){
        if (gameObject.transform.parent == null){
            gameObject.transform.parent = Camera.main.transform;
            var mainTransform = transform;
            mainTransform.localPosition = Vector3.zero;
            mainTransform.localRotation = Quaternion.identity;
            mainTransform.localScale = Vector3.one;
            IsHeadFixed = true;
            Debug.Log("Calibration grid parent: Camera.main.transform;"); 
        }
        else{
             gameObject.transform.parent = null;
             IsHeadFixed = false;
             Debug.Log("Calibration grid parent: null;"); 
            //  Vector3 currentPos = GameObject.transform.position;
            //  gameObject.transform.position = new Vector3( currentPos.x, Camera.main.transform.position.y, currentPos.z); // set to headheight
            //  gameObject.transform.rotation = Quaternion.identity;
        }
    }
    
    public void LateUpdate()
    {
        if (Physics.Raycast(vrCamera.transform.position,
            vrCamera.transform.TransformDirection(eyeTracker.ChosenTracker.GetLocalGazeDirection()), out var hit))
        {
            gazeToTargetDist = Vector3.Angle(CurrentTargetTransform.localPosition, vrCamera.transform.InverseTransformPoint(hit.point));
        }

        gazeErrorText.text = "Current Error: " + gazeToTargetDist.ToString("0.#") + " degrees";
    }
}

