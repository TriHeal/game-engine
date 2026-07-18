using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component now lives DIRECTLY on the Avatar Select Screen GameObject.
/// It triggers its initial setup and checks whenever it is enabled.
/// </summary>
public class AvatarSelectController : MonoBehaviour
{
    [Header("Navigation Links")]
    [Tooltip("The Main Game/Dashboard view to activate next.")]
    public GameObject selectGameScreen;
    [Tooltip("Optional CanvasGroup on the game screen for smooth cross-fading.")]
    public CanvasGroup selectGameGroup;

    [Header("Fade & Canvas")]
    private CanvasGroup myCanvasGroup;
    public float crossFadeDuration = 0.5f;

    [Header("Avatar Stage")]
    [Tooltip("Disabled until this screen is active.")]
    public Camera stageCamera;
    [Tooltip("One instance per avatar. Only the current index is active at a time.")]
    public GameObject[] avatarInstances;

    [Header("Catalog Setup")]
    public AvatarCatalog catalog;
    public string[] avatarIds = { "cat", "duck", "mole_monster", "sheep" };
    public string[] avatarDisplayNames = { "חתול", "ברווז", "חפרפרת", "כבשה" };

    [Header("UI Controls")]
    public TMP_Text nameText;
    public Image[] dotIndicators;
    public Color dotActiveColor = new Color(0.98f, 0.96f, 0.9f, 1f);
    public Color dotInactiveColor = new Color(0.98f, 0.96f, 0.9f, 0.35f);
    public Button prevButton;
    public Button nextButton;
    public Button selectButton;

    private int currentIndex;
    private bool transitioning;

    private void Awake()
    {
        // Cache our own CanvasGroup component attached to this GameObject
        myCanvasGroup = GetComponent<CanvasGroup>();
        
        // Setup Button Listeners once
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (selectButton != null) selectButton.onClick.AddListener(Select);

        // Build mapping from catalog
        if (catalog != null && catalog.entries != null && catalog.entries.Length > 0)
        {
            avatarIds = new string[catalog.entries.Length];
            avatarDisplayNames = new string[catalog.entries.Length];
            for (int i = 0; i < catalog.entries.Length; i++)
            {
                avatarIds[i] = catalog.entries[i].id;
                avatarDisplayNames[i] = catalog.entries[i].displayName;
            }
        }
    }

    private void OnEnable()
    {
        // SAFETY: If a character is already selected and we aren't deliberately forcing a change,
        // bypass this screen entirely and forward straight to the Game Selection menu.
        if (AvatarSession.Instance != null && AvatarSession.Instance.HasSelectedAvatar())
        {
            Debug.Log($"[AvatarSelect] Already has Avatar, cross-fading to Game Selection Screen.");
            if (selectGameScreen != null) selectGameScreen.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        // Reset state upon showing up
        transitioning = false;
        currentIndex = 0;
        
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = 1f;
            myCanvasGroup.interactable = true;
            myCanvasGroup.blocksRaycasts = true;
        }

        if (stageCamera != null) stageCamera.enabled = true;

        RefreshAvatar();
        Debug.Log("[AvatarSelect] OnEnable -> Resetting layout and activating stage camera.");
    }

    private void OnDisable()
    {
        // Clean up when screen becomes inactive to save mobile performance
        if (stageCamera != null) stageCamera.enabled = false;
        StopAllCoroutines();
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

    public void Select()
    {
        if (transitioning || avatarInstances.Length == 0) return;
        transitioning = true;

        string id = currentIndex < avatarIds.Length ? avatarIds[currentIndex] : null;
        if (!string.IsNullOrEmpty(id))
        {
            AvatarSession.Instance.SetSelectedAvatar(id);
        }

        Debug.Log($"[AvatarSelect] Confirmed -> chosen '{id}', cross-fading to Game Selection Screen.");
        StartCoroutine(CrossFadeToGameSelection());
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

    private IEnumerator CrossFadeToGameSelection()
    {
        if (myCanvasGroup != null)
        {
            myCanvasGroup.interactable = false;
            myCanvasGroup.blocksRaycasts = false;
        }

        if (selectGameScreen != null) selectGameScreen.SetActive(true);
        if (selectGameGroup != null) selectGameGroup.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, crossFadeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            
            if (myCanvasGroup != null) myCanvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            if (selectGameGroup != null) selectGameGroup.alpha = eased;
            
            yield return null;
        }

        if (selectGameGroup != null) selectGameGroup.alpha = 1f;
        
        // Disable ourselves now that we are done!
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tri-Heal/Avatar/Clear Selected Avatar")]
    private static void ClearSelectedAvatar()
    {
        PlayerPrefs.DeleteKey(AvatarSession.SelectedAvatarKey);
        PlayerPrefs.Save();
        Debug.Log("[AvatarSelect] Cleared selected avatar pref");
    }
#endif
}