using UnityEngine;

/// <summary>
/// Replaces a hardcoded placeholder avatar prop (e.g. the duck sitting on the
/// raft) with the child's actually-selected avatar from AvatarSession, matching
/// the placeholder's position/rotation/scale exactly so it drops into the same
/// spot. AvatarSession always falls back to the catalog's first entry when
/// nothing's been picked yet, so this normally always has something to show.
/// </summary>
public class HomeAvatarDisplay : MonoBehaviour
{
    [Tooltip("Placeholder avatar prop to replace (e.g. the raft's duck instance). Destroyed once the real selection spawns in its place.")]
    public GameObject placeholder;

    void Start()
    {
        if (placeholder == null) return;

        var spawned = AvatarSession.Instance.SpawnAvatar(placeholder.transform.parent);
        if (spawned == null) return;

        spawned.transform.SetLocalPositionAndRotation(placeholder.transform.localPosition, placeholder.transform.localRotation);
        spawned.transform.localScale = placeholder.transform.localScale;
        spawned.name = "SelectedAvatar";

        Destroy(placeholder);
    }
}
