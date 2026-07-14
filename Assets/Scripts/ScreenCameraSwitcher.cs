using UnityEngine;

/// <summary>
/// Drop on any screen GameObject (Canvas root) that needs a specific 3D
/// camera rig active behind it -- e.g. the games-selection hub wants the
/// zoomed-out world view, while the breathing exercise wants the raft-follow
/// view. Swaps on OnEnable so each screen owns its own camera state without
/// the screens' controllers needing to know about each other.
/// </summary>
public class ScreenCameraSwitcher : MonoBehaviour
{
    public GameObject[] activateOnEnable;
    public GameObject[] deactivateOnEnable;

    void OnEnable()
    {
        foreach (var go in deactivateOnEnable)
            if (go != null) go.SetActive(false);

        foreach (var go in activateOnEnable)
            if (go != null) go.SetActive(true);
    }
}
