using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Draggable bubble on the Step 3 sort screen (Screen Space - Overlay Canvas).
/// Holds its own correct category so SortScreenController can validate drops.
/// </summary>
public class EDISortableBubble : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Category { Fact, Thought }

    [Header("Refs")]
    public TextMeshProUGUI label;
    public CanvasGroup canvasGroup;

    public Category category { get; private set; }
    public bool locked { get; private set; }

    private RectTransform rect;
    private Canvas canvas;
    private Vector2 homeAnchoredPos;
    private SortScreenController controller;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(string text, Category cat, SortScreenController owner)
    {
        if (label != null)
            label.text = text;

        category = cat;
        controller = owner;
        locked = false;
        homeAnchoredPos = rect.anchoredPosition;
    }

    public void SnapHome()
    {
        rect.anchoredPosition = homeAnchoredPos;
    }

    public void Lock()
    {
        locked = true;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (locked) return;
        if (canvasGroup != null)
            canvasGroup.alpha = 0.85f;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (locked) return;
        rect.anchoredPosition += eventData.delta / (canvas != null ? canvas.scaleFactor : 1f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (locked) return;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        controller.HandleDrop(this, eventData);
    }

    public void ReturnHome()
    {
        rect.anchoredPosition = homeAnchoredPos;
    }
}
