using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns one EDI ("Emotional Debugging") rock event: cracking the rock open
/// (step 1->2) and sorting the resulting fact/thought bubbles (step 2->3).
/// Content is hardcoded test data for now, independent of the rock's own
/// engraved text. Triggered by a temporary in-scene "Separate" button standing
/// in for the therapist's real control.
/// </summary>
public class EDIEventController : MonoBehaviour
{
    [Header("Bubble Content (test data)")]
    public string originalText = "שני ילדים לחשו אחד לשני בהפסקה";
    public string[] factOptions = new string[]
    {
        "הם דיברו עליי",
        "כולם שונאים אותי",
    };
    public string[] thoughtOptions = new string[]
    {
        "שני ילדים לחשו אחד לשני",
        "הם לא רוצים אותי",
    };

    [Header("Rock Halves")]
    public Transform halfRockLeft;
    public Transform halfRockRight;
    public float separationDistance = 1.2f;
    public float separationDuration = 1f;

    [Header("Glow / VFX")]
    public ParticleSystem glowVfxPrefab;
    public float glowSpawnProgress = 0.3f;

    [Header("Bubbles")]
    public EDIBubble bubblePrefab;
    public Transform crackAnchor;
    public float topBubbleHeight = 1.0f;
    public float bottomBubbleSpacing = 0.6f;
    public Vector3 bubbleFinalScale = Vector3.one;

    [Header("Step 3")]
    public SortScreenController sortScreen;
    public float delayBeforeSort = 1.5f;

    [Header("UI")]
    public GameObject separateButton;

    private bool started;

    public void Separate()
    {
        if (started) return;
        started = true;

        if (separateButton != null)
            separateButton.SetActive(false);

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        yield return StartCoroutine(SeparateHalves());
        SpawnBubbles();
        yield return new WaitForSeconds(delayBeforeSort);
        OpenSortScreen();
    }

    private IEnumerator SeparateHalves()
    {
        Vector3 leftStart = halfRockLeft.localPosition;
        Vector3 rightStart = halfRockRight.localPosition;

        Vector3 apart = (rightStart - leftStart).normalized * (separationDistance * 0.5f);
        bool glowSpawned = false;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / separationDuration;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            halfRockLeft.localPosition = leftStart - apart * eased;
            halfRockRight.localPosition = rightStart + apart * eased;

            if (!glowSpawned && t >= glowSpawnProgress)
            {
                glowSpawned = true;
                SpawnGlow();
            }

            yield return null;
        }
    }

    private void SpawnGlow()
    {
        if (glowVfxPrefab == null) return;
        Vector3 pos = crackAnchor != null ? crackAnchor.position : transform.position;
        Instantiate(glowVfxPrefab, pos, Quaternion.identity);
    }

    private void SpawnBubbles()
    {
        if (bubblePrefab == null || crackAnchor == null) return;

        List<(string text, EDISortableBubble.Category category)> shuffled = BuildShuffledOptions();

        // Top bubble: original text
        EDIBubble top = Instantiate(bubblePrefab, crackAnchor.parent);
        top.transform.position = crackAnchor.position;
        Vector3 topTarget = crackAnchor.localPosition + Vector3.up * topBubbleHeight;
        top.Setup(originalText, crackAnchor.localPosition, topTarget, bubbleFinalScale);

        // Bottom row: 4 shuffled options
        float startX = -bottomBubbleSpacing * 1.5f;
        for (int i = 0; i < shuffled.Count; i++)
        {
            EDIBubble bubble = Instantiate(bubblePrefab, crackAnchor.parent);
            Vector3 target = crackAnchor.localPosition + new Vector3(startX + i * bottomBubbleSpacing, -0.3f, 0f);
            bubble.Setup(shuffled[i].text, crackAnchor.localPosition, target, bubbleFinalScale);
        }

        pendingOptions = shuffled;
    }

    private List<(string text, EDISortableBubble.Category category)> pendingOptions;

    private List<(string text, EDISortableBubble.Category category)> BuildShuffledOptions()
    {
        List<(string text, EDISortableBubble.Category category)> options = new List<(string, EDISortableBubble.Category)>();

        foreach (string fact in factOptions)
            options.Add((fact, EDISortableBubble.Category.Fact));
        foreach (string thought in thoughtOptions)
            options.Add((thought, EDISortableBubble.Category.Thought));

        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        return options;
    }

    private void OpenSortScreen()
    {
        if (sortScreen == null || pendingOptions == null) return;
        sortScreen.Show(pendingOptions);
    }
}
