using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreenController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject breathingScreen;

    [Tooltip("BreathingCircle on the breathing screen's Orb, reset when leaving the screen")]
    public BreathingCircle breathingCircle;

    public void OpenBreathing()
    {
        homeScreen.SetActive(false);
        breathingScreen.SetActive(true);
        if (breathingCircle != null) breathingCircle.StartExercise();
    }

    public void OpenStoneBreak()
    {
        SceneManager.LoadScene("rocksFlow");
    }

    public void BackToHome()
    {
        breathingScreen.SetActive(false);
        homeScreen.SetActive(true);
        if (breathingCircle != null) breathingCircle.StopExercise();
    }
}