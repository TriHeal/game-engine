using UnityEngine;

/// <summary>
/// Single source of truth for the pickable avatars (id, Hebrew display name,
/// prefab). Lives as one shared asset so AvatarSelectController and every
/// mini-game read the same list instead of each keeping its own copy.
/// </summary>
[CreateAssetMenu(fileName = "AvatarCatalog", menuName = "Tri-Heal/Avatar Catalog")]
public class AvatarCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;
        public string displayName;
        public GameObject prefab;

        [Tooltip("Added on top of wherever a consumer places this avatar. Most prefabs have their root at their feet (offset stays zero); a few (e.g. the mole) have their root elsewhere, so this corrects for that per-prefab quirk once, here, instead of every spawn site re-discovering it.")]
        public Vector3 spawnOffset;
    }

    public Entry[] entries = new Entry[0];

    public Entry Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var entry in entries)
        {
            if (entry != null && entry.id == id) return entry;
        }
        return null;
    }

    /// <summary>Looks up by id, falling back to the first entry (e.g. before any selection exists).</summary>
    public Entry GetOrDefault(string id)
    {
        var entry = Get(id);
        if (entry != null) return entry;
        return entries.Length > 0 ? entries[0] : null;
    }
}
