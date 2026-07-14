using UnityEngine;
using Unity.Cinemachine; // Cinemachine v3 namespace

public class SplineCameraController : MonoBehaviour
{
    [Header("Cinemachine Connections")]
    public CinemachineCamera virtualCamera; // Drag your Cinemachine Camera here

    [Header("Movement Settings")]
    [Tooltip("How fast the camera moves along the spline with swipes")]
    public float swipeSensitivity = 0.5f;

    [Tooltip("How smoothly the camera stops sliding after a swipe")]
    public float inertiaDamping = 5f;

    private CinemachineSplineDolly dolly;
    private Vector2 lastInputPosition;
    private bool isDragging = false;
    private float currentVelocity = 0f;

    private void Start()
    {
        if (virtualCamera != null)
        {
            dolly = virtualCamera.GetComponent<CinemachineSplineDolly>();
        }
    }

    private void LateUpdate()
    {
        if (dolly == null) return;

        HandleInput();
        ApplyInertia();
    }

    private void HandleInput()
    {
        // 1. Mobile Touch Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastInputPosition = touch.position;
                currentVelocity = 0f;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = touch.position.x - lastInputPosition.x;
                MovePath(deltaX);
                lastInputPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // 2. Mouse Input (for Testing in Unity Editor)
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastInputPosition = Input.mousePosition;
                currentVelocity = 0f;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                float deltaX = Input.mousePosition.x - lastInputPosition.x;
                MovePath(deltaX);
                lastInputPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    private void MovePath(float deltaX)
    {
        float deltaNormalized = (deltaX / Screen.width) * swipeSensitivity;
        currentVelocity = -deltaNormalized; 

        // Normalized position along spline (0.0 to 1.0)
        dolly.CameraPosition += currentVelocity;
        ClampPathPosition();
    }

    private void ApplyInertia()
    {
        if (!isDragging && Mathf.Abs(currentVelocity) > 0.0001f)
        {
            dolly.CameraPosition += currentVelocity;
            currentVelocity = Mathf.Lerp(currentVelocity, 0f, Time.deltaTime * inertiaDamping);
            ClampPathPosition();
        }
    }

    private void ClampPathPosition()
    {
        // Keeps position between start (0) and end (1) of the spline
        dolly.CameraPosition = Mathf.Clamp01(dolly.CameraPosition);
    }
}