using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// The raft-follow camera is shared by WelcomeScreen, the breathing exercise,
/// and HomeScreen. Home wants the raft/avatar pulled into the bottom-left of
/// the frame (so the mini-games buttons sit in open center space) without
/// affecting the other screens' normal centered composition. Drop this on
/// HomeScreen: it overrides the shared camera's Composer screen position
/// while Home is active and restores the default the moment it isn't.
/// </summary>
public class HomeCameraFraming : MonoBehaviour
{
    public CinemachineRotationComposer composer;
    public Vector2 homeScreenPosition = new Vector2(-0.3f, 0.3f);
    public Vector2 defaultScreenPosition = Vector2.zero;

    void OnEnable() => SetScreenPosition(homeScreenPosition);

    void OnDisable() => SetScreenPosition(defaultScreenPosition);

    private void SetScreenPosition(Vector2 position)
    {
        if (composer == null) return;
        var composition = composer.Composition;
        composition.ScreenPosition = position;
        composer.Composition = composition;
    }
}
