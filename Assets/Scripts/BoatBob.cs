using UnityEngine;

public class BoatBob : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float tiltAmount = 3f;

    private Vector3 startLocalPos;

    private void Start()
    {
        // Store the LOCAL starting position relative to the parent
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float t = Time.time * bobSpeed;
        
        // 1. Handle Bobbing locally
        if (bobHeight != 0)
        {
            float yOffset = Mathf.Sin(t) * bobHeight;
            transform.localPosition = startLocalPos + new Vector3(0f, yOffset, 0f);
        }

        // 2. Handle Tilting locally (X-axis for pitching, or Z-axis for rolling)
        float tilt = Mathf.Sin(t) * tiltAmount;
        
        // Using localRotation ensures it tilts relative to the direction the spline is facing
        transform.localRotation = Quaternion.Euler(tilt, 0f, 0f);
    }
}