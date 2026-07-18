using UnityEngine;

/// <summary>
/// Drop on the HomeScreen canvas root. Tells HomeAvatarDisplay to re-check the
/// current AvatarSession selection every time this screen becomes visible --
/// covers both the first arrival from AvatarSelectScreen and returning here
/// via BackToHome().
/// </summary>
public class HomeAvatarRefresher : MonoBehaviour
{
    public HomeAvatarDisplay avatarDisplay;

    void OnEnable()
    {
        if (avatarDisplay != null) avatarDisplay.Refresh();
    }
}
