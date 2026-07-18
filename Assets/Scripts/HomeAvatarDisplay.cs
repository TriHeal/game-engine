using UnityEngine;

/// <summary>
/// Replaces a hardcoded placeholder avatar prop (e.g. the duck sitting on the
/// raft) with the child's actually-selected avatar from AvatarSession, matching
/// the placeholder's position/rotation/scale exactly so it drops into the same
/// spot. AvatarSession always falls back to the catalog's first entry when
/// nothing's been picked yet, so this normally always has something to show.
///
/// The raft itself is never toggled off/on, so Start() alone would only ever
/// show whatever was selected *before* this scene loaded. Call Refresh()
/// whenever the home screen becomes visible (see HomeAvatarRefresher) so a
/// selection made earlier in the same session is picked up too.
/// </summary>
public class HomeAvatarDisplay : MonoBehaviour
{
    [Tooltip("Placeholder avatar prop to replace on first refresh (e.g. the raft's duck instance).")]
    public GameObject placeholder;
    public bool faceCamera = true;

    private Transform anchor;
    private Vector3 anchorLocalPosition;
    private Quaternion anchorLocalRotation;
    private Vector3 anchorLocalScale;

    private GameObject spawned;
    private string spawnedAvatarId;

    void Start()
    {
        if (placeholder != null)
        {
            Debug.Log("Got placeholder");
            anchor = placeholder.transform.parent;
            if (anchor == null)
            {
                Debug.Log("anchor is null");
            }
            anchorLocalPosition = placeholder.transform.localPosition;
            anchorLocalRotation = placeholder.transform.localRotation;
            anchorLocalScale = placeholder.transform.localScale;
        }

        Refresh();
    }

    /// <summary>Re-spawns the displayed avatar if the session's selection has changed since the last refresh.</summary>
    public void Refresh()
    {
        if (anchor == null) return;

        var entry = AvatarSession.Instance.SelectedEntry;
        string id = entry?.id;
        if (spawned != null && spawnedAvatarId == id) return;

        var newSpawn = AvatarSession.Instance.SpawnAvatar(anchor);
        if (newSpawn == null) return;

        newSpawn.transform.SetLocalPositionAndRotation(anchorLocalPosition, anchorLocalRotation);
        newSpawn.transform.localScale = anchorLocalScale;

        if (faceCamera)
        {
            // Face the Home screen's camera directly, independent of whatever
            // rotation the raft itself has. Camera.main's *position* is the same
            // regardless of which screen's framing is active (only its rotation
            // changes, via the Cinemachine composer's screen-position offset), so
            // this stays correct without needing to know about that offset.
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 toCamera = mainCam.transform.position - newSpawn.transform.position;
                toCamera.y = 0f;
                if (toCamera.sqrMagnitude > 0.0001f)
                    newSpawn.transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
        }

        // spawnOffset is a mesh-space correction (how far a prefab's root sits
        // from its feet) measured at the prefab's native scale, so it has to be
        // scaled by this instance's actual rendered size before being added in
        // world space -- otherwise it's either multiplied by the parent's scale
        // twice (as a raw local offset) or not at all (as a raw world offset),
        // both of which over/under-correct once the avatar is scaled down to
        // fit the raft.
        if (entry != null && entry.spawnOffset != Vector3.zero)
            newSpawn.transform.position += Vector3.Scale(entry.spawnOffset, newSpawn.transform.lossyScale);

        newSpawn.name = "SelectedAvatar";

        if (spawned != null) Destroy(spawned);
        else if (placeholder != null) Destroy(placeholder);

        spawned = newSpawn;
        spawnedAvatarId = id;

        Debug.Log("Avatar loaded");
    }
}
