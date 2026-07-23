using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Shows wellDoneUI when raftSplineAnimate finishes its journey (Completed
/// fires once, since Loop is Once), holds it for a fixed duration, then
/// hides it again. Lives on an always-active GameObject (the Raft) so the
/// Completed subscription is live regardless of wellDoneUI's own shown/
/// hidden state.
/// </summary>
public class WellDoneOnComplete : MonoBehaviour
{
    public SplineAnimate raftSplineAnimate;
    public GameObject wellDoneUI;
    public float displayDuration = 3f;

    private void OnEnable()
    {
        if (raftSplineAnimate != null)
            raftSplineAnimate.Completed += HandleCompleted;
    }

    private void OnDisable()
    {
        if (raftSplineAnimate != null)
            raftSplineAnimate.Completed -= HandleCompleted;
    }

    private void HandleCompleted()
    {
        if (wellDoneUI != null)
            StartCoroutine(ShowThenHide());
    }

    private IEnumerator ShowThenHide()
    {
        wellDoneUI.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        wellDoneUI.SetActive(false);
    }
}
