using UnityEngine;
using System;

public class DuckJumpController : MonoBehaviour
{
    public float jumpDuration = 0.8f;
    public float jumpHeight = 1.8f;
    public LayerMask rockLayer;

    public bool IsJumping { get; private set; }

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float jumpTimer = 0f;
    private Action onJumpComplete;

    public void JumpTo(Vector3 target, Action onComplete = null)
    {
        startPosition = transform.position;
        targetPosition = target;
        jumpTimer = 0f;
        onJumpComplete = onComplete;
        IsJumping = true;
    }

    void Update()
    {
        if (IsJumping)
        {
            jumpTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(jumpTimer / jumpDuration);

            Vector3 currentGroundPos = Vector3.Lerp(startPosition, targetPosition, progress);
            float currentHeight = 4f * jumpHeight * progress * (1f - progress);

            transform.position = new Vector3(currentGroundPos.x, currentGroundPos.y + currentHeight, currentGroundPos.z);

            Vector3 lookDir = (targetPosition - startPosition);
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, progress * 3f);
            }

            if (progress >= 1f)
            {
                IsJumping = false;
                transform.position = targetPosition;
                onJumpComplete?.Invoke();
            }
        }
    }
}