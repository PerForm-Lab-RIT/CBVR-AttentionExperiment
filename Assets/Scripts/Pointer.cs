using UnityEngine;
using Valve.VR;

public class Pointer : MonoBehaviour
{
    [SerializeField] private float thickness;
    [SerializeField] private Material pointerMaterial;
    [SerializeField] private GameObject selectionCircle;
    [SerializeField] private GameObject arrow;
    [SerializeField] private float controllerDeadzone;

    [SerializeField] private SteamVR_Action_Vector2 arrowRotate;
    [SerializeField] private SteamVR_Input_Sources currentHand;
    private GameObject _pointer;

    private const float DefaultDistance = 100f;
    private const string PointerObjectName = "Laser";

    public void Start()
    {
        _pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _pointer.name = PointerObjectName;
        _pointer.transform.localScale = new Vector3(thickness, thickness, DefaultDistance);
        _pointer.transform.localPosition = new Vector3(0f, 0f, DefaultDistance / 2f);
        _pointer.transform.localRotation = Quaternion.Euler(90, 0, 0);
        _pointer.transform.SetParent(transform);
        _pointer.GetComponent<MeshRenderer>().material = pointerMaterial;
        var pointerCollider = _pointer.GetComponent<CapsuleCollider>();
        Destroy(pointerCollider);
    }

    public void Update()
    {
        var controllerTransform = transform;
        var raycast = new Ray(controllerTransform.position, controllerTransform.forward);
        var hasHit = Physics.Raycast(raycast, out var hit);
        var laserDistance = DefaultDistance;

        if (hasHit)
        {
            laserDistance = hit.distance;
            selectionCircle.SetActive(true);
            
            var selectionCircleTransform = selectionCircle.transform;
            selectionCircleTransform.position = hit.point;
            selectionCircleTransform.rotation = Quaternion.LookRotation(hit.normal);
            UpdateArrow();
        }
        else
        {
            selectionCircle.SetActive(false);
        }

        _pointer.transform.localScale = new Vector3(thickness, laserDistance / 2f, thickness);
        _pointer.transform.localPosition = new Vector3(0f, 0f, laserDistance / 2f);
    }

    private void UpdateArrow()
    {
        var currentDirection = arrowRotate.GetAxis(currentHand);
        if (currentDirection.sqrMagnitude > controllerDeadzone * controllerDeadzone)
        {
            arrow.SetActive(true);
            var currentRotation = Mathf.Acos(Vector2.Dot(Vector3.up, currentDirection.normalized))
                * 180f / Mathf.PI;
            if (currentDirection.x < 0)
                currentRotation = -currentRotation;
            arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 180f + currentRotation);
        }
        else
        {
            arrow.SetActive(false);
        }
    }
}
