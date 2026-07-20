using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This component now lives DIRECTLY on the Select Game Screen (Home Screen) GameObject.
/// It automatically handles refreshing elements or state when it becomes active.
/// </summary>
public class HomeScreenController : MonoBehaviour
{
    [Header("Sub-Screens / Overlays")]
    public GameObject AvatarSelectScreen;

    [Header("Dependencies")]
    [Tooltip("BreathingCircle on the breathing screen's Orb, reset when leaving the screen.")]
    public BreathingCircle breathingCircle;

    private void OnEnable()
    {
        // This runs instantly every time you return from another game scene 
        // or transition from the character picker screen.
        Debug.Log("[HomeScreen] Activated -> Ready for game selection.");
    }

    /// <summary>Wired to your Breathing Game button.</summary>
    public void OpenBreathing()
    {
        SceneManager.LoadScene("SyncRiver");
    }

    /// <summary>Wired to the Stone Breaking Game button.</summary>
    public void OpenStoneBreak()
    {
        SceneManager.LoadScene("StoneFlow");
    }

    /// <summary>Wired to the Memory Lake Game button.</summary>
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