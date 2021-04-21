using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    
    private void FixedUpdate()
    {
        var o = gameObject;
        o.transform.position = cameraTransform.position;
        o.transform.rotation = cameraTransform.rotation;
    }
}
