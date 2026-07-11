using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Carousel avatar picker shown once between Welcome and Home. One 3D animal
/// (cat / duck / mole / sheep) is on stage at a time, playing its idle animation;
/// Prev/Next cycle through them and Select confirms and cross-fades into
/// <see cref="nextScreen"/> (Home today).
///
/// Entry fade-in is owned by the previous screen's controller (wired via its own
/// "nextGroup" pointing at <see cref="avatarGroup"/>), matching the Welcome/Login
/// pattern. This controller only owns fading itself back out on Select.
/// </summary>
public class AvatarSelectController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject avatarSelectScreen;

    [Tooltip("What Select opens (Home).")]
    public GameObject nextScreen;

    [Header("Fade")]
    public CanvasGroup avatarGroup;

    [Tooltip("Optional CanvasGroup on the next screen. If null, the next screen simply activates under the fading-out avatar picker.")]
    public CanvasGroup nextGroup;

    public float crossFadeDuration = 0.8f;

    [Header("Avatar Stage")]
    [Tooltip("Disabled until this screen is actually shown, enabled again while it's up.")]
    public Camera stageCamera;

    [Tooltip("One instance per avatar, same order as avatarIds/avatarDisplayNames. Only the current index is active at a time.")]
    public GameObject[] avatarInstances;

    public string[] avatarIds = { "cat", "duck", "mole_monster", "sheep" };
    public string[] avatarDisplayNames = { "חתול", "ברווז", "חפרפרת", "כבשה" };

    [Header("UI")]
    public TMP_Text nameText;
    public Image[] dotIndicators;
    public Color dotActiveColor = new Color(0.98f, 0.96f, 0.9f, 1f);
    public Color dotInactiveColor = new Color(0.98f, 0.96f, 0.9f, 0.35f);
    public Button prevButton;
    public Button nextButton;
    public Button selectButton;

    public const string SelectedAvatarKey = "SelectedAvatarId";

    private int currentIndex;
    private bool wasScreenActive;
    private bool transitioning;

    void Start()
    {
        if (avatarSelectScreen != null) avatarSelectScreen.SetActive(true);
        if (nextScreen != null) nextScreen.SetActive(false);

        if (avatarGroup != null)
        {
            avatarGroup.alpha = 0f;
            avatarGroup.interactable = false;
            avatarGroup.blocksRaycasts = true;
        }

        if (stageCamera != null) stageCamera.enabled = false;

        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (selectButton != null) selectButton.onClick.AddListener(Select);

        RefreshAvatar();

        // avatarSelectScreen starts active (mirrors Welcome/Login) but invisible
        // behind alpha 0 until the previous screen's cross-fade shows it.
        wasScreenActive = avatarSelectScreen != null && avatarSelectScreen.activeSelf;
    }

    void Update()
    {
        bool nowActive = avatarSelectScreen != null && avatarSelectScreen.activeSelf;
        if (nowActive && !wasScreenActive) OnShown();
        wasScreenActive = nowActive;
    }

    private void OnShown()
    {
        transitioning = false;
        currentIndex = 0;
        RefreshAvatar();
        if (stageCamera != null) stageCamera.enabled = true;
        if (avatarGroup != null)
        {
            avatarGroup.interactable = true;
            avatarGroup.blocksRaycasts = true;
        }
        Debug.Log("[AvatarSelect] Shown -> stage camera on, index reset to 0");
    }

    public void Next()
    {
        if (transitioning || avatarInstances.Length == 0) return;
        currentIndex = (currentIndex + 1) % avatarInstances.Length;
        RefreshAvatar();
    }

    public void Prev()
    {
        if (transitioning || avatarInstances.Length == 0) return;
        currentIndex = (currentIndex - 1 + avatarInstances.Length) % avatarInstances.Length;
        RefreshAvatar();
    }

    /// <summary>Wired to the Select button's OnClick.</summary>
    public void Select()
    {
        if (transitioning || avatarInstances.Length == 0) return;
        transitioning = true;

        string id = currentIndex < avatarIds.Length ? avatarIds[currentIndex] : null;
        if (!string.IsNullOrEmpty(id))
        {
            PlayerPrefs.SetString(SelectedAvatarKey, id);
            PlayerPrefs.Save();
        }

        Debug.Log($"[AvatarSelect] Select -> chose '{id}', cross-fading to next screen");
        StartCoroutine(CrossFade());
    }

    private void RefreshAvatar()
    {
        for (int i = 0; i < avatarInstances.Length; i++)
        {
            if (avatarInstances[i] != null) avatarInstances[i].SetActive(i == currentIndex);
        }

        if (nameText != null && currentIndex < avatarDisplayNames.Length)
            nameText.text = avatarDisplayNames[currentIndex];

        for (int i = 0; i < dotIndicators.Length; i++)
        {
            if (dotIndicators[i] != null)
                dotIndicators[i].color = i == currentIndex ? dotActiveColor : dotInactiveColor;
        }
    }

    private IEnumerator CrossFade()
    {
        if (avatarGroup != null)
        {
            avatarGroup.interactable = false;
            avatarGroup.blocksRaycasts = false;
        }

        if (nextScreen != null) nextScreen.SetActive(true);
        if (nextGroup != null) nextGroup.alpha = 0f;

        float startAlpha = avatarGroup != null ? avatarGroup.alpha : 1f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, crossFadeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            if (avatarGroup != null) avatarGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);
            if (nextGroup != null) nextGroup.alpha = eased;
            yield return null;
        }

        if (nextGroup != null) nextGroup.alpha = 1f;
        if (stageCamera != null) stageCamera.enabled = false;
        if (avatarSelectScreen != null) avatarSelectScreen.SetActive(false);
        Debug.Log("[AvatarSelect] Cross-fade complete -> avatar picker hidden, next screen active");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tri-Heal/Avatar/Clear Selected Avatar")]
    private static void ClearSelectedAvatar()
    {
        PlayerPrefs.DeleteKey(SelectedAvatarKey);
        PlayerPrefs.Save();
        Debug.Log("[AvatarSelect] Cleared selected avatar pref");
    }
#endif
}
