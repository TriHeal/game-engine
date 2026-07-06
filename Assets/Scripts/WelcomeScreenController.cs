using System.Collections;
using UnityEngine;

/// <summary>
/// Calm entry "front door" for the child client. Fades the Hebrew welcome panel
/// in over the shared water/boat backdrop, eases an ambient loop up, and on the
/// CTA cross-fades into the next screen.
///
/// The "what comes next" is a single swappable <see cref="nextScreen"/> reference
/// (the Home hub today), so a future continuous river-journey / session-entry flow
/// can replace the hub without reworking this screen. Welcome is an entry gate
/// shown once at launch; returning from activities lands on Home, not here.
/// </summary>
public class WelcomeScreenController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject welcomeScreen;

    [Tooltip("What the CTA opens. The Home hub today; swap for session-entry later.")]
    public GameObject nextScreen;

    [Header("Fade")]
    public CanvasGroup welcomeGroup;

    [Tooltip("Optional CanvasGroup on the next screen, faded in during the cross-fade for a true blend. If null, the next screen simply activates under the fading-out welcome.")]
    public CanvasGroup nextGroup;

    public float fadeInDuration = 1.5f;
    public float holdBeforeInteractive = 0.3f;
    public float crossFadeDuration = 0.8f;

    [Header("Ambient Audio (placeholder loop until a real track exists)")]
    public AudioSource ambient;
    public float ambientTargetVolume = 0.6f;
    public float audioFadeInDuration = 3f;

    [Header("Gentle Motion")]
    [Tooltip("Optional focal point (e.g. the CTA or orb/glow) given a soft breathing pulse while the welcome screen is up.")]
    public Transform breathePulseTarget;
    public float pulseAmount = 0.04f;
    public float pulseSpeed = 1.2f;

    [Header("Future hook")]
    [Tooltip("Mediating-character avatar. Unused for now; drop the art in here later.")]
    public GameObject avatar;

    private bool transitioning;
    private Vector3 pulseBaseScale = Vector3.one;

    void Start()
    {
        if (welcomeScreen != null) welcomeScreen.SetActive(true);
        if (nextScreen != null) nextScreen.SetActive(false);

        if (welcomeGroup != null)
        {
            welcomeGroup.alpha = 0f;
            welcomeGroup.interactable = false;
            welcomeGroup.blocksRaycasts = false;
        }

        if (breathePulseTarget != null)
            pulseBaseScale = breathePulseTarget.localScale;

        Debug.Log($"[Welcome] Start -> fading in (welcomeGroup={(welcomeGroup != null ? "OK" : "NULL")}, ambient={(ambient != null ? "OK" : "NULL")}, nextScreen={(nextScreen != null ? nextScreen.name : "NULL")})");

        StartCoroutine(FadeIn());
        StartCoroutine(FadeInAudio());
    }

    void Update()
    {
        // Soft breathing pulse on the focal point while the welcome screen is up.
        if (breathePulseTarget != null && !transitioning)
        {
            float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            breathePulseTarget.localScale = pulseBaseScale * s;
        }
    }

    /// <summary>Wire this to the CTA button's OnClick.</summary>
    public void Continue()
    {
        if (transitioning) return;
        transitioning = true;
        Debug.Log("[Welcome] Continue -> cross-fading to next screen");
        StartCoroutine(CrossFade());
    }

    private IEnumerator FadeIn()
    {
        yield return FadeGroup(welcomeGroup, 0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdBeforeInteractive);
        if (welcomeGroup != null)
        {
            welcomeGroup.interactable = true;
            welcomeGroup.blocksRaycasts = true;
        }
    }

    private IEnumerator FadeInAudio()
    {
        if (ambient == null) yield break;

        ambient.loop = true;
        ambient.volume = 0f;
        if (!ambient.isPlaying) ambient.Play();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, audioFadeInDuration);
            ambient.volume = Mathf.Lerp(0f, ambientTargetVolume, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }
        ambient.volume = ambientTargetVolume;
    }

    private IEnumerator CrossFade()
    {
        if (welcomeGroup != null)
        {
            welcomeGroup.interactable = false;
            welcomeGroup.blocksRaycasts = false;
        }

        if (nextScreen != null) nextScreen.SetActive(true);
        if (nextGroup != null) nextGroup.alpha = 0f;

        float startWelcome = welcomeGroup != null ? welcomeGroup.alpha : 1f;
        float startVolume = ambient != null ? ambient.volume : 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, crossFadeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            if (welcomeGroup != null) welcomeGroup.alpha = Mathf.Lerp(startWelcome, 0f, eased);
            if (nextGroup != null) nextGroup.alpha = eased;
            // The ambient loop belongs to the welcome screen, so retire it with the
            // transition instead of letting it bleed under the next screen.
            if (ambient != null) ambient.volume = Mathf.Lerp(startVolume, 0f, eased);
            yield return null;
        }

        if (nextGroup != null) nextGroup.alpha = 1f;
        if (ambient != null) { ambient.volume = 0f; ambient.Stop(); }
        if (breathePulseTarget != null) breathePulseTarget.localScale = pulseBaseScale;
        if (welcomeScreen != null) welcomeScreen.SetActive(false);
        Debug.Log("[Welcome] Cross-fade complete -> welcome hidden, ambient stopped, next screen active");
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        group.alpha = from;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }
        group.alpha = to;
    }
}
