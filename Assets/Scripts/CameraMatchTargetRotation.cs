using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Rigidly locks the vcam's rotation to a source Transform (plus a fixed local
/// offset), instead of aiming at a world-space point. Used so the StoneFlow
/// camera keeps the same relative over-the-shoulder angle on the raft while it
/// travels down the river, rather than swinging to track a fixed target.
/// </summary>
[AddComponentMenu("")]
public class CameraMatchTargetRotation : CinemachineExtension
{
    public Transform rotationSource;
    public Quaternion localOffset = Quaternion.identity;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Aim && rotationSource != null)
            state.RawOrientation = rotationSource.rotation * localOffset;
    }
}
