using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Step 3 of the EDI flow: a Screen Space - Overlay canvas with two drop zones
/// ("what really happened" / "what my brain might think"). Spawns the four
/// shuffled option bubbles and validates each drop against its true category.
/// </summary>
public class SortScreenController : MonoBehaviour
{
    [Header("Refs")]
    public EDISortDropZone factZone;
    public EDISortDropZone thoughtZone;
    public EDISortableBubble bubblePrefab;
    public RectTransform spawnRow;

    [Header("Events")]
    public UnityEvent OnSortComplete;

    private readonly List<EDISortableBubble> bubbles = new List<EDISortableBubble>();
    private int correctCount;

    public void Show(List<(string text, EDISortableBubble.Category category)> options)
    {
        gameObject.SetActive(true);
        Clear();

        correctCount = 0;

        float spacing = spawnRow.rect.width / (options.Count + 1);
        for (int i = 0; i < options.Count; i++)
        {
            EDISortableBubble bubble = Instantiate(bubblePrefab, spawnRow);
            RectTransform r = (RectTransform)bubble.transform;
            r.anchoredPosition = new Vector2(spacing * (i + 1) - spawnRow.rect.width * 0.5f, 0f);

            bubble.Setup(options[i].text, options[i].category, this);
            bubbles.Add(bubble);
        }
    }

    public void HandleDrop(EDISortableBubble bubble, PointerEventData eventData)
    {
        EDISortDropZone zone = ResolveZone(eventData);

        if (zone != null && zone.acceptedCategory == bubble.category)
        {
            SnapIntoZone(bubble, zone);
            bubble.Lock();
            correctCount++;

            if (correctCount >= bubbles.Count)
            {
                Debug.Log("EDI sort complete: all bubbles placed correctly.");
                OnSortComplete?.Invoke();
            }
        }
        else
        {
            bubble.ReturnHome();
        }
    }

    private EDISortDropZone ResolveZone(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(factZone.rect, eventData.position))
            return factZone;
        if (RectTransformUtility.RectangleContainsScreenPoint(thoughtZone.rect, eventData.position))
            return thoughtZone;
        return null;
    }

    private void SnapIntoZone(EDISortableBubble bubble, EDISortDropZone zone)
    {
        RectTransform target = zone.slotsContainer != null ? zone.slotsContainer : zone.rect;
        bubble.transform.SetParent(target, worldPositionStays: false);

        int slotIndex = target.childCount - 1;
        ((RectTransform)bubble.transform).anchoredPosition = new Vector2(slotIndex * 140f, 0f);
    }

    private void Clear()
    {
        foreach (EDISortableBubble b in bubbles)
            if (b != null)
                Destroy(b.gameObject);
        bubbles.Clear();
    }
}
