using UnityEngine;

using UnityEngine.UI;

public class BreathingCircle : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.4f;

    public float inhaleDuration = 4f;
    public float holdFullDuration = 1f;
    public float exhaleDuration = 6f;
    public float holdEmptyDuration = 1f;

    public Image holdGlowImage;
    private float phaseTimer = 0f;

    public float maxDragDistance = 160f;

    private enum BreathPhase
    {
        Inhale,
        HoldFull,
        Exhale,
        HoldEmpty
    }

    private BreathPhase currentPhase = BreathPhase.Inhale;

    void Update()
    {
        phaseTimer += Time.deltaTime;

        switch (currentPhase)
        {
            case BreathPhase.Inhale:
                AnimateScale(minScale, maxScale, phaseTimer / inhaleDuration);

                if (phaseTimer >= inhaleDuration)
                    MoveToNextPhase(BreathPhase.HoldFull);
                break;

            case BreathPhase.HoldFull:
                SetScale(maxScale);

                if (phaseTimer >= holdFullDuration)
                    MoveToNextPhase(BreathPhase.Exhale);
                break;

            case BreathPhase.Exhale:
                AnimateScale(maxScale, minScale, phaseTimer / exhaleDuration);

                if (phaseTimer >= exhaleDuration)
                    MoveToNextPhase(BreathPhase.HoldEmpty);
                break;

            case BreathPhase.HoldEmpty:
                SetScale(minScale);

                if (phaseTimer >= holdEmptyDuration)
                    MoveToNextPhase(BreathPhase.Inhale);
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

    private void MoveToNextPhase(BreathPhase nextPhase)
    {
        currentPhase = nextPhase;
        phaseTimer = 0f;
    }

    private void UpdateGlow()
    {
       if (holdGlowImage == null)
        return;

        Color color = holdGlowImage.color;

        float targetAlpha = currentPhase == BreathPhase.HoldFull ? 0.75f : 0f;
        float targetScale = currentPhase == BreathPhase.HoldFull ? 1.08f : 1f;

        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
        holdGlowImage.transform.localScale = Vector3.Lerp(
            holdGlowImage.transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * 5f
        );

        holdGlowImage.color = color;
    }

}