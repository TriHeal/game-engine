using UnityEngine;

/// <summary>
/// Persistent, self-creating singleton holding the child's chosen avatar for
/// the whole app run. Any mini-game scene can call AvatarSession.Instance to
/// read the selection or spawn a copy of the chosen avatar, without needing
/// its own copy of the avatar list or its own PlayerPrefs plumbing.
///
/// Self-creates on first access (no bootstrapper GameObject required in every
/// scene) and survives scene loads via DontDestroyOnLoad, so it keeps working
/// whether a mini-game is reached through the normal Home flow or opened
/// directly for isolated testing.
/// </summary>
public class AvatarSession : MonoBehaviour
{
    public const string SelectedAvatarKey = "SelectedAvatarId";
    private const string CatalogResourcePath = "AvatarCatalog";

    private static AvatarSession instance;

    public static AvatarSession Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AvatarSession>();
                if (instance == null)
                {
                    var go = new GameObject(nameof(AvatarSession));
                    instance = go.AddComponent<AvatarSession>();
                }
            }
            return instance;
        }
    }

    [Tooltip("Falls back to Resources/AvatarCatalog.asset when left empty.")]
    [SerializeField] private AvatarCatalog catalog;

    public AvatarCatalog Catalog => catalog;
    public string SelectedAvatarId { get; private set; }
    public AvatarCatalog.Entry SelectedEntry => catalog != null ? catalog.GetOrDefault(SelectedAvatarId) : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (catalog == null)
            catalog = Resources.Load<AvatarCatalog>(CatalogResourcePath);

        SelectedAvatarId = PlayerPrefs.GetString(SelectedAvatarKey, string.Empty);
    }

    /// <summary>Persists the choice and updates the live session value. Wired from AvatarSelectController.Select().</summary>
    public void SetSelectedAvatar(string id)
    {
        SelectedAvatarId = id;
        PlayerPrefs.SetString(SelectedAvatarKey, id);
        PlayerPrefs.Save();
        Debug.Log($"[AvatarSession] Selected avatar set to '{id}'");
    }

    /// <summary>Spawns a copy of the chosen avatar's prefab (or the catalog's first entry as a fallback). Caller positions/parents it.</summary>
    public GameObject SpawnAvatar(Transform parent = null)
    {
        var entry = SelectedEntry;
        if (entry == null || entry.prefab == null)
        {
            Debug.LogWarning("[AvatarSession] No avatar selected/catalog entry found; nothing to spawn.");
            return null;
        }

        return Instantiate(entry.prefab, parent);
    }
}
