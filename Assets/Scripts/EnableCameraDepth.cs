using UnityEngine;

[ExecuteAlways]
public class EnableCameraDepth : MonoBehaviour
{
    private void OnEnable()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }
    }
}