using UnityEngine;

public class AttachToMainCamera : MonoBehaviour
{
    void Start()
    {
        if (!(Camera.main is null)) this.transform.SetParent(Camera.main.transform, false);
    }
}
