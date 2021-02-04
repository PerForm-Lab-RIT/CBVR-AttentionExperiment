using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] private float thickness;
    [SerializeField] private Material pointerMaterial;
    [SerializeField] private MeshRenderer selectionCircle;
    [SerializeField] private Material selectionCircleMaterial;

    private GameObject _pointer;

    private const float DefaultDistance = 100f;
    
    public void Start()
    {
        _pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _pointer.name = "Laser Pointer";
        _pointer.transform.localScale = new Vector3(thickness, thickness, DefaultDistance);
        _pointer.transform.localPosition = new Vector3(0f, 0f, DefaultDistance / 2f);
        _pointer.transform.localRotation = Quaternion.Euler(90, 0, 0);
        _pointer.transform.SetParent(transform);
        _pointer.GetComponent<MeshRenderer>().material = pointerMaterial;
        var pointerCollider = _pointer.GetComponent<CapsuleCollider>();
        Destroy(pointerCollider);
        selectionCircle.material = selectionCircleMaterial;
    }
    
    public void Update()
    {
        var controllerTransform = transform;
        var raycast = new Ray(controllerTransform.position, controllerTransform.forward);
        var hasHit = Physics.Raycast(raycast, out var hit);
        var dist = DefaultDistance;

        if (hasHit && hit.distance > 0.1f)
        {
            dist = hit.distance;
            selectionCircle.enabled = true;
            
            var selectionCircleTransform = selectionCircle.transform;
            selectionCircleTransform.position = hit.point;
            selectionCircleTransform.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            selectionCircle.enabled = false;
        }

        _pointer.transform.localScale = new Vector3(thickness, dist / 2f, thickness);
        _pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        
    }
}
