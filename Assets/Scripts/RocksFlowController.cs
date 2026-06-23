using UnityEngine;
using UnityEngine.SceneManagement;

public class RocksFlowController : MonoBehaviour
{
    public void BackToHome()
    {
        SceneManager.LoadScene("main");
    }
}
