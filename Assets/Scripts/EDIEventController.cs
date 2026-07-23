using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

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
        "אני לא יודע על מה הם דברו",
        "שני ילדים לחשו אחד לשני",
    };
    public string[] thoughtOptions = new string[]
    {
        "כולם שונאים אותי",
        "הם לא רוצים אותי",
    };

    [Header("Rock Halves")]
    public Transform halfRockLeft;
    public Transform halfRockRight;
    public float separationDistance = 1.2f;
    public float separationDuration = 1f;

    [Header("Bubbles")]
    public EDIBubble bubblePrefab;
    public EDIBubble titleBubblePrefab;
    public Transform crackAnchor;
    public float topBubbleHeight = 1.0f;
    public float bottomBubbleSpacing = 0.6f;
    public Vector3 bubbleFinalScale = Vector3.one;

    [Header("Engraved Rock Text")]
    public GameObject engravedRockText;
    public GameObject engravedRockPanel;

    [Header("Step 3")]
    public SortScreenController sortScreen;
    public float delayBeforeSort = 1.5f;

    [Header("Step 4: Clear the Path")]
    public SplineAnimate raftSplineAnimate;
    public float crackAnchorGrowScale = 6f;
    public float pushApartDistance = 4f;
    public float clearPathDuration = 1.5f;
    public float baselineSpeed = 0.5f;

    [Header("UI")]
    public GameObject separateButton;

    [Header("Well Done Card")]
    [Tooltip("Root of the WellDoneCard/BackBtn canvas, shown only while the path is being cleared.")]
    public GameObject wellDoneUI;

    private bool started;
    private string cachedOriginalText;

    void Start()
    {
        if (sortScreen != null)
            sortScreen.OnSortComplete.AddListener(OnSortingComplete);
    }

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
        if (engravedRockText != null)
        {
            // Save the engraved text itself (not a hardcoded copy of it) before
            // hiding it, so the bubbles/sort screen show exactly what's on the rock.
            TMPro.TextMeshPro tmp = engravedRockText.GetComponent<TMPro.TextMeshPro>();
            cachedOriginalText = tmp != null ? tmp.text : originalText;
            
            engravedRockText.SetActive(false);
            engravedRockPanel.SetActive(false);
        }

        Vector3 leftStart = halfRockLeft.localPosition;
        Vector3 rightStart = halfRockRight.localPosition;

        Vector3 apart = (rightStart - leftStart).normalized * (separationDistance * 0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / separationDuration;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            halfRockLeft.localPosition = leftStart - apart * eased;
            halfRockRight.localPosition = rightStart + apart * eased;

            yield return null;
        }
    }

    private void SpawnBubbles()
    {
        if (bubblePrefab == null || crackAnchor == null) return;

        List<(string text, EDISortableBubble.Category category)> shuffled = BuildShuffledOptions();

        // Top bubble: original text, styled as a title using the wood-panel UI.
        // Tracked so OpenSortScreen() can clear it too — it's redundant once the
        // sort screen's header shows the same text.
        // EDIBubble top = Instantiate(titleBubblePrefab != null ? titleBubblePrefab : bubblePrefab, crackAnchor.parent);
        // top.transform.position = crackAnchor.position;
        // Vector3 topTarget = crackAnchor.localPosition + Vector3.up * topBubbleHeight;
        // top.Setup(cachedOriginalText, crackAnchor.localPosition, topTarget, bubbleFinalScale);
        // worldTitleBubble = top;

        // The fact/thought options themselves are never shown in world space —
        // they go straight to the sort screen's SpawnRow, so the player never
        // sees a redundant preview before dragging them.
        pendingOptions = shuffled;
    }

    private List<(string text, EDISortableBubble.Category category)> pendingOptions;
    // private EDIBubble worldTitleBubble;

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

        // if (worldTitleBubble != null)
        // {
        //     Destroy(worldTitleBubble.gameObject);
        //     worldTitleBubble = null;
        // }

        // The engraved text + its backdrop panel move to the canvas too, as
        // the sort screen's header.
        if (engravedRockText != null)
            engravedRockText.SetActive(false);
        if (engravedRockPanel != null)
            engravedRockPanel.SetActive(false);

        sortScreen.Show(cachedOriginalText, pendingOptions);
    }

    private void OnSortingComplete()
    {
        if (sortScreen != null)
            sortScreen.gameObject.SetActive(false);

        StartCoroutine(ClearPath());
    }

    private IEnumerator ClearPath()
    {
        if (wellDoneUI != null)
            wellDoneUI.SetActive(true);

        yield return new WaitForSeconds(delayBeforeSort);

        Vector3 anchorStartScale = crackAnchor.localScale;
        Vector3 anchorTargetScale = anchorStartScale * crackAnchorGrowScale;

        Vector3 leftStart = halfRockLeft.localPosition;
        Vector3 rightStart = halfRockRight.localPosition;
        Vector3 apart = (rightStart - leftStart).normalized * pushApartDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / clearPathDuration;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            crackAnchor.localScale = Vector3.LerpUnclamped(anchorStartScale, anchorTargetScale, eased);
            halfRockLeft.localPosition = leftStart - apart * eased;
            halfRockRight.localPosition = rightStart + apart * eased;

            yield return null;
        }

        // // Path is clear: let the raft resume its journey down the river.
        if (raftSplineAnimate != null)
        {
            raftSplineAnimate.enabled = true;
            raftSplineAnimate.MaxSpeed = baselineSpeed;
            raftSplineAnimate.NormalizedTime = 0f;
            raftSplineAnimate.ElapsedTime = 0f;
            
            raftSplineAnimate.Play();
        }

        if (wellDoneUI != null)
            wellDoneUI.SetActive(false);
    }
}
