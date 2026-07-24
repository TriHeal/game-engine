using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the activity-selection screen.
/// Only activities selected by the therapist for the current session are shown.
/// </summary>
public class HomeScreenController : MonoBehaviour
{
    [Header("Sub-Screens / Overlays")]
    public GameObject AvatarSelectScreen;

    [Header("Activity Buttons")]
    public GameObject breathingButton;
    public GameObject stoneBreakButton;
    public GameObject memoryLakeButton;
    public GameObject bondingForestButton;

    [Header("Dependencies")]
    [Tooltip("BreathingCircle on the breathing screen's Orb, reset when leaving the screen.")]
    public BreathingCircle breathingCircle;

    private void OnEnable()
    {
        SessionContext.Changed += RefreshAvailableActivities;
        RealtimeSessionListener.ActivityChanged += RefreshAvailableActivities;

        Debug.Log("[HomeScreen] Activated -> refreshing activity states.");
        RefreshAvailableActivities();
    }

    private void OnDisable()
    {
        SessionContext.Changed -= RefreshAvailableActivities;
        RealtimeSessionListener.ActivityChanged -= RefreshAvailableActivities;
    }

    private void RefreshAvailableActivities()
    {
        if (!SessionContext.Load())
        {
            SetActivityState(breathingButton, false, false);
            SetActivityState(stoneBreakButton, false, false);
            SetActivityState(memoryLakeButton, false, false);
            SetActivityState(bondingForestButton, false, false);
            return;
        }

        bool breathingSelected = HasAnyActivity("breathing");
        bool rocksSelected = HasAnyActivity("event_processing");
        bool memorySelected = HasAnyActivity("memory_lake", "memory_book");
        bool bondingSelected = HasAnyActivity("bonding_forest", "tree_forest");

        SetActivityState(
            breathingButton,
            breathingSelected,
            RealtimeSessionListener.IsActivityActive("breathing")
        );

        SetActivityState(
            stoneBreakButton,
            rocksSelected,
            RealtimeSessionListener.IsActivityActive("event_processing")
        );

        SetActivityState(
            memoryLakeButton,
            memorySelected,
            RealtimeSessionListener.IsActivityActive("memory_lake") ||
            RealtimeSessionListener.IsActivityActive("memory_book")
        );

        SetActivityState(
            bondingForestButton,
            bondingSelected,
            RealtimeSessionListener.IsActivityActive("bonding_forest") ||
            RealtimeSessionListener.IsActivityActive("tree_forest")
        );
    }
    private bool HasAnyActivity(params string[] activityTypes)
    {
        foreach (string activityType in activityTypes)
        {
            if (SessionContext.HasActivity(activityType))
            {
                return true;
            }
        }

        return false;
    }
    
    private void SetActivityState(
    GameObject activityButton,
    bool selected,
    bool active
    )
    {
        if (activityButton == null)
        {
            return;
        }

        activityButton.SetActive(true);

        Button button = activityButton.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogWarning(
                $"[HomeScreen] No Button component found on {activityButton.name}."
            );
            return;
        }

        CanvasGroup canvasGroup = activityButton.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = activityButton.AddComponent<CanvasGroup>();
        }

        bool clickable = selected && active;

        button.interactable = clickable;
        canvasGroup.interactable = clickable;
        canvasGroup.blocksRaycasts = clickable;

        // Only activities not selected for the session are greyed out.
        canvasGroup.alpha = selected ? 1f : 0.4f;
    }

    public void OpenBreathing()
    {
        SceneManager.LoadScene("SyncRiver");
    }

    public void OpenStoneBreak()
    {
        SceneManager.LoadScene("StoneFlow");
    }

    public void OpenMemoryLake()
    {
        SceneManager.LoadScene("MemoryLake");
    }

    public void OpenBondingForest()
    {
        SceneManager.LoadScene("BondingForest");
    }

    public void OpenSelectAvatar()
    {
        if (AvatarSelectScreen != null)
        {
            AvatarSession.Instance.ClearSavedAvatar();
            gameObject.SetActive(false);
            AvatarSelectScreen.SetActive(true);
        }
    }
}