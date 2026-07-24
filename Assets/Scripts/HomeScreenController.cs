using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the activity-selection screen.
/// Activities selected for the current session are enabled immediately.
/// Activities not selected by the therapist remain greyed out and disabled.
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

        Debug.Log(
            "[HomeScreen] Activated -> refreshing selected activities."
        );

        RefreshAvailableActivities();
    }

    private void OnDisable()
    {
        SessionContext.Changed -= RefreshAvailableActivities;
    }

    private void RefreshAvailableActivities()
    {
        if (!SessionContext.Load())
        {
            SetActivityState(breathingButton, false);
            SetActivityState(stoneBreakButton, false);
            SetActivityState(memoryLakeButton, false);
            SetActivityState(bondingForestButton, false);
            return;
        }

        bool breathingSelected =
            HasAnyActivity("breathing");

        bool rocksSelected =
            HasAnyActivity("event_processing");

        bool memorySelected =
            HasAnyActivity(
                "memory_lake",
                "memory_book"
            );

        bool bondingSelected =
            HasAnyActivity(
                "bonding_forest",
                "tree_forest"
            );

        SetActivityState(
            breathingButton,
            breathingSelected
        );

        SetActivityState(
            stoneBreakButton,
            rocksSelected
        );

        SetActivityState(
            memoryLakeButton,
            memorySelected
        );

        SetActivityState(
            bondingForestButton,
            bondingSelected
        );
    }

    private bool HasAnyActivity(
        params string[] activityTypes
    )
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
        bool selected
    )
    {
        if (activityButton == null)
        {
            return;
        }

        activityButton.SetActive(true);

        Button button =
            activityButton.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogWarning(
                $"[HomeScreen] No Button component found on {activityButton.name}."
            );

            return;
        }

        CanvasGroup canvasGroup =
            activityButton.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                activityButton.AddComponent<CanvasGroup>();
        }

        button.interactable = selected;
        canvasGroup.interactable = selected;
        canvasGroup.blocksRaycasts = selected;
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
        if (AvatarSelectScreen == null)
        {
            return;
        }

        AvatarSession.Instance.ClearSavedAvatar();

        gameObject.SetActive(false);
        AvatarSelectScreen.SetActive(true);
    }
}
