using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform mainCam;

    void Start()
    {
        if (Camera.main != null) mainCam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            transform.LookAt(transform.position + mainCam.rotation * Vector3.forward,
                             mainCam.rotation * Vector3.up);
        }
    }
}