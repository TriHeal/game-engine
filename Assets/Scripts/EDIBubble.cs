using UnityEngine;
using TMPro;

/// <summary>
/// A single world-space bubble (TMP text on a sprite/quad) used in the EDI
/// step-2 burst: one "original text" bubble plus four shuffled option bubbles.
/// Animates a scale-in bounce from a spawn point to its target local position.
/// </summary>
public class EDIBubble : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshPro label;

    [Header("Spawn Animation")]
    public float growDuration = 0.45f;
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 spawnLocalPos;
    private Vector3 targetLocalPos;
    private Vector3 targetLocalScale;
    private float t;
    private bool animating;

    public void Setup(string text, Vector3 spawnPos, Vector3 targetPos, Vector3 finalScale)
    {
        if (label != null)
            label.text = text;

        spawnLocalPos = spawnPos;
        targetLocalPos = targetPos;
        targetLocalScale = finalScale;

        transform.localPosition = spawnLocalPos;
        transform.localScale = Vector3.zero;

        t = 0f;
        animating = true;
    }

    void Update()
    {
        if (!animating) return;

        t += Time.deltaTime / growDuration;
        float eval = growCurve.Evaluate(Mathf.Clamp01(t));

        transform.localPosition = Vector3.LerpUnclamped(spawnLocalPos, targetLocalPos, eval);
        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetLocalScale, eval);

        if (t >= 1f)
        {
            transform.localPosition = targetLocalPos;
            transform.localScale = targetLocalScale;
            animating = false;
        }
    }
}
