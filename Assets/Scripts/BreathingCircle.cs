using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BreathingCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float minScale = 6.2f;
    public float maxScale = 9.3f;

    public float growDuration = 4f;
    public float holdAirDuration = 1.5f;
    public float shrinkDuration = 6f;

    [Header("Grace Period (early release)")]
    public float gracePeriodDuration = 2f;
    public float graceShrinkSpeed = 0.3f;

    public Image holdGlowImage;
    public Text counterText;
    public Image progressRingImage;
    public Text phaseLabelText;

    [Tooltip("SailWind controller")]
    public SailWind saildWind;

    [Tooltip("Shower emission controller")]
    public ShowerEmissionController showerController;

    private int phaseCounter = 0;

    private const string InhaleLabel = "שאיפה";
    private const string HoldLabel = "החזקה";
    private const string ExhaleLabel = "נשיפה";

    private enum BreathState
    {
        Idle,
        Growing,
        HoldingAir,
        Grace,
        Releasing
    }

    private BreathState currentState = BreathState.Idle;
    private float phaseTimer = 0f;

    private BreathState savedStateForGrace;
    private float savedTimerForGrace;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == BreathState.Idle)
        {
            currentState = BreathState.Growing;
            phaseTimer = 0f;
        }
        else if (currentState == BreathState.Grace)
        {
            // Resume exactly where the finger left off - no reset, no penalty.
            currentState = savedStateForGrace;
            phaseTimer = savedTimerForGrace;
        }
        // Mid Growing/HoldingAir/Releasing: ignore, already in progress.
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentState == BreathState.Growing || currentState == BreathState.HoldingAir)
        {
            savedStateForGrace = currentState;
            savedTimerForGrace = phaseTimer;
            currentState = BreathState.Grace;
            phaseTimer = 0f;
        }
        // Idle/Releasing: releasing the finger has no effect.
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;

        switch (currentState)
        {
            case BreathState.Idle:
                SetScale(minScale);
                SetCounterText(null);
                SetProgressRing(false, 0f);
                SetPhaseLabel(null);
                break;

            case BreathState.Growing:
                float growProgress = Mathf.Clamp01(phaseTimer / growDuration);
                AnimateScale(minScale, maxScale, growProgress);
                SetCounterNumber(Mathf.Clamp(Mathf.FloorToInt(growProgress * 4f) + 1, 1, 4));
                SetProgressRing(true, growProgress);
                SetPhaseLabel(InhaleLabel);

                if (phaseTimer >= growDuration)
                    MoveToNextPhase(BreathState.HoldingAir);
                break;

            case BreathState.HoldingAir:
                SetScale(maxScale);
                SetCounterText(null);
                SetProgressRing(true, 1f);
                SetPhaseLabel(HoldLabel);

                if (phaseTimer >= holdAirDuration)
                {
                    MoveToNextPhase(BreathState.Releasing);
                    AdvancePhase();
                }
                break;

            case BreathState.Grace:
                float graceScale = Mathf.MoveTowards(transform.localScale.x, minScale, graceShrinkSpeed * Time.deltaTime);
                SetScale(graceScale);
                // Counter, ring and label intentionally left untouched - stay frozen at their last value.

                if (phaseTimer >= gracePeriodDuration)
                {
                    MoveToNextPhase(BreathState.Idle);
                    RetreatPhase();
                }
                break;

            case BreathState.Releasing:
                float shrinkProgress = Mathf.Clamp01(phaseTimer / shrinkDuration);
                AnimateScale(maxScale, minScale, shrinkProgress);
                SetCounterNumber(Mathf.Clamp(6 - Mathf.FloorToInt(shrinkProgress * 6f), 1, 6));
                SetProgressRing(true, 1f - shrinkProgress);
                SetPhaseLabel(ExhaleLabel);

                if (phaseTimer >= shrinkDuration)
                    MoveToNextPhase(BreathState.Idle);
                break;
        }

        UpdateGlow();
    }

    private void AdvancePhase()
    {
        phaseCounter++;
        Debug.Log($"[Breathing] AdvancePhase -> phaseCounter={phaseCounter}, state={currentState}, saildWind={(saildWind != null ? "OK" : "NULL")}, showerController={(showerController != null ? "OK" : "NULL")}");
        if (saildWind != null) saildWind.SetByCycleCounter(phaseCounter);
        if (showerController != null) showerController.IncreaseLevel();
    }

    private void RetreatPhase()
    {
        phaseCounter = Mathf.Max(0, phaseCounter - 1);
        Debug.Log($"[Breathing] Interrupted -> phaseCounter={phaseCounter}, saildWind={(saildWind != null ? "OK" : "NULL")}, showerController={(showerController != null ? "OK" : "NULL")}");
        if (saildWind != null) saildWind.SetByCycleCounter(phaseCounter);
        if (showerController != null) showerController.DecreaseLevel();
    }

    public void ResetExercise()
    {
        currentState = BreathState.Idle;
        phaseTimer = 0f;
        phaseCounter = 0;
        Debug.Log("[Breathing] ResetExercise -> back to Idle, phaseCounter=0");
        if (saildWind != null) saildWind.SetByCycleCounter(0);
        if (showerController != null) showerController.ResetLevel();
    }

    private void AnimateScale(float from, float to, float progress)
    {
        progress = Mathf.Clamp01(progress);
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
        float scale = Mathf.Lerp(from, to, smoothProgress);
        SetScale(scale);
    }

    private void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    private void MoveToNextPhase(BreathState nextState)
    {
        currentState = nextState;
        phaseTimer = 0f;
    }

    private void SetCounterNumber(int number)
    {
        if (counterText == null)
            return;

        counterText.text = number.ToString();
    }

    private void SetCounterText(string text)
    {
        if (counterText == null)
            return;

        counterText.text = text ?? string.Empty;
    }

    private void SetPhaseLabel(string label)
    {
        if (phaseLabelText == null)
            return;

        // Legacy UI.Text always lays out glyphs left-to-right, even for Hebrew.
        // Reversing the characters here is what actually makes it read correctly right-to-left.
        phaseLabelText.text = label == null ? string.Empty : ReverseForRtlDisplay(label);
    }

    private static string ReverseForRtlDisplay(string text)
    {
        char[] characters = text.ToCharArray();
        System.Array.Reverse(characters);
        return new string(characters);
    }

    private void SetProgressRing(bool visible, float fillAmount)
    {
        if (progressRingImage == null)
            return;

        if (progressRingImage.gameObject.activeSelf != visible)
            progressRingImage.gameObject.SetActive(visible);

        progressRingImage.fillAmount = fillAmount;
    }

    private void UpdateGlow()
    {
        if (holdGlowImage == null)
            return;

        Color color = holdGlowImage.color;

        float targetAlpha = currentState == BreathState.HoldingAir ? 0.75f : 0f;
        float targetScale = currentState == BreathState.HoldingAir ? 1.08f : 1f;

        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
        holdGlowImage.transform.localScale = Vector3.Lerp(
            holdGlowImage.transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * 5f
        );

        holdGlowImage.color = color;
    }
}
