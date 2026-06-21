using UnityEngine;

/// <summary>
/// Marks a UI panel on the Step 3 sort screen as a valid drop target for a
/// specific bubble category ("what really happened" vs "what my brain thinks").
/// </summary>
public class EDISortDropZone : MonoBehaviour
{
    public EDISortableBubble.Category acceptedCategory;
    public RectTransform slotsContainer;

    public RectTransform rect => (RectTransform)transform;
}
