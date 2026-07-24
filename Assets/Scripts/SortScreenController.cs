using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Step 3 of the EDI flow: displays the fact/thought sorting screen,
/// creates draggable bubbles, and validates their destination.
/// </summary>
public class SortScreenController : MonoBehaviour
{
    [Header("Refs")]
    public EDISortDropZone factZone;
    public EDISortDropZone thoughtZone;
    public EDISortableBubble bubblePrefab;
    public RectTransform spawnRow;

    [Header("Header")]
    public TextMeshProUGUI headerText;

    [Header("Events")]
    public UnityEvent OnSortComplete;

    private readonly List<EDISortableBubble> bubbles =
        new List<EDISortableBubble>();

    private int correctCount;

    public void Show(
        string headerLabel,
        List<(string text, EDISortableBubble.Category category)> options
    )
    {
        gameObject.SetActive(true);

        Clear();

        if (headerText != null)
        {
            headerText.text = headerLabel;
        }

        if (bubblePrefab == null)
        {
            Debug.LogError(
                "[RocksFlow] SortScreen bubblePrefab is not assigned."
            );
            return;
        }

        if (spawnRow == null)
        {
            Debug.LogError(
                "[RocksFlow] SortScreen spawnRow is not assigned."
            );
            return;
        }

        if (options == null || options.Count == 0)
        {
            Debug.LogWarning(
                "[RocksFlow] SortScreen received no options."
            );
            return;
        }

        correctCount = 0;

        // Ensure the newly enabled canvas has calculated its dimensions.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(spawnRow);

        // Keep the bubbles above the large fact/thought panels.
        spawnRow.SetAsLastSibling();

        float rowWidth = spawnRow.rect.width;

        if (rowWidth <= 1f)
        {
            Debug.LogWarning(
                "[RocksFlow] spawnRow width was not calculated; using fallback width."
            );

            rowWidth = 900f;
        }

        float spacing = rowWidth / (options.Count + 1);

        for (int i = 0; i < options.Count; i++)
        {
            EDISortableBubble bubble =
                Instantiate(bubblePrefab, spawnRow, false);

            // An inactive scene template creates inactive clones.
            bubble.gameObject.SetActive(true);

            RectTransform rect =
                bubble.GetComponent<RectTransform>();

            if (rect == null)
            {
                Debug.LogError(
                    "[RocksFlow] Bubble prefab has no RectTransform."
                );

                Destroy(bubble.gameObject);
                continue;
            }

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            rect.anchoredPosition = new Vector2(
                spacing * (i + 1) - rowWidth * 0.5f,
                0f
            );

            if (bubble.canvasGroup != null)
            {
                bubble.canvasGroup.alpha = 1f;
                bubble.canvasGroup.interactable = true;
                bubble.canvasGroup.blocksRaycasts = true;
            }

            bubble.Setup(
                options[i].text,
                options[i].category,
                this
            );

            bubble.transform.SetAsLastSibling();
            bubbles.Add(bubble);

            Debug.Log(
                $"[RocksFlow] Created bubble: " +
                $"category={options[i].category}, " +
                $"text={options[i].text}, " +
                $"position={rect.anchoredPosition}"
            );
        }

        Canvas.ForceUpdateCanvases();

        Debug.Log(
            $"[RocksFlow] Sort screen opened with {bubbles.Count} bubbles."
        );
    }

    public void HandleDrop(
        EDISortableBubble bubble,
        PointerEventData eventData
    )
    {
        EDISortDropZone zone = ResolveZone(eventData);

        if (
            zone != null &&
            zone.acceptedCategory == bubble.category
        )
        {
            SnapIntoZone(bubble, zone);
            bubble.Lock();

            correctCount++;

            if (correctCount >= bubbles.Count)
            {
                Debug.Log(
                    "[RocksFlow] All bubbles placed correctly."
                );

                OnSortComplete?.Invoke();
            }
        }
        else
        {
            bubble.ReturnHome();
        }
    }

    private EDISortDropZone ResolveZone(
        PointerEventData eventData
    )
    {
        if (
            factZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                factZone.rect,
                eventData.position
            )
        )
        {
            return factZone;
        }

        if (
            thoughtZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                thoughtZone.rect,
                eventData.position
            )
        )
        {
            return thoughtZone;
        }

        return null;
    }

    private void SnapIntoZone(
        EDISortableBubble bubble,
        EDISortDropZone zone
    )
    {
        RectTransform target =
            zone.slotsContainer != null
                ? zone.slotsContainer
                : zone.rect;

        bubble.transform.SetParent(
            target,
            false
        );

        int slotIndex = target.childCount - 1;

        RectTransform bubbleRect =
            bubble.GetComponent<RectTransform>();

        bubbleRect.anchoredPosition =
            new Vector2(
                0f,
                50f - slotIndex * 100f
            );
    }

    private void Clear()
    {
        foreach (EDISortableBubble bubble in bubbles)
        {
            if (bubble != null)
            {
                Destroy(bubble.gameObject);
            }
        }

        bubbles.Clear();
    }
}
