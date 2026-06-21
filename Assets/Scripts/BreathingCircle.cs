using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BreathingCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float minScale = 4f;
    public float maxScale = 6f;

    public float growDuration = 4f;
    public float holdAirDuration = 1.5f;
    public float shrinkDuration = 6f;

    [Header("Grace Period (early release)")]
    public float gracePeriodDuration = 2f;
    public float graceShrinkSpeed = 0.3f;

    public Image holdGlowImage;
    public Text counterText;

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
                break;

            case BreathState.Growing:
                AnimateScale(minScale, maxScale, phaseTimer / growDuration);
                SetCounterNumber(Mathf.Clamp(Mathf.FloorToInt(phaseTimer / growDuration * 4f) + 1, 1, 4));

                if (phaseTimer >= growDuration)
                    MoveToNextPhase(BreathState.HoldingAir);
                break;

            case BreathState.HoldingAir:
                SetScale(maxScale);
                SetCounterText(null);

                if (phaseTimer >= holdAirDuration)
                    MoveToNextPhase(BreathState.Releasing);
                break;

            case BreathState.Grace:
                float graceScale = Mathf.MoveTowards(transform.localScale.x, minScale, graceShrinkSpeed * Time.deltaTime);
                SetScale(graceScale);
                // Counter intentionally left untouched - stays frozen at its last value.

                if (phaseTimer >= gracePeriodDuration)
                    MoveToNextPhase(BreathState.Idle);
                break;

            case BreathState.Releasing:
                AnimateScale(maxScale, minScale, phaseTimer / shrinkDuration);
                SetCounterNumber(Mathf.Clamp(6 - Mathf.FloorToInt(phaseTimer / shrinkDuration * 6f), 1, 6));

                if (phaseTimer >= shrinkDuration)
                    MoveToNextPhase(BreathState.Idle);
                break;
        }

        UpdateGlow();
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
