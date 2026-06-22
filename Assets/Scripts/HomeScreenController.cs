using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreenController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject breathingScreen;

    public void OpenBreathing()
    {
        homeScreen.SetActive(false);
        breathingScreen.SetActive(true);
    }

    public void OpenStoneBreak()
    {
        SceneManager.LoadScene("rocksFlow");
    }
}