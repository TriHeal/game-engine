using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldCanvasPanner : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Target UI")]
    [Tooltip("Drag the parent panel RectTransform that should shift upward when typing.")]
    public RectTransform panelToMove;

    [Header("Padding & Animation")]
    [Tooltip("Extra space in Canvas units above the keyboard edge.")]
    public float marginAboveKeyboard = 40f;
    public float lerpSpeed = 12f;

    [Header("Fallback Height")]
    [Tooltip("Percentage of screen height to move in Editor or if OS fails to report.")]
    [Range(0.05f, 0.5f)]
    public float fallbackScreenPercentage = 0.35f;

    private TMP_InputField inputField;
    private RectTransform inputRect;
    private Vector2 originalPanelPos;
    private bool isFocused;
    private Coroutine animateCoroutine;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputRect = GetComponent<RectTransform>();

        if (panelToMove != null)
        {
            originalPanelPos = panelToMove.anchoredPosition;
        }
    }

    private void OnDisable()
    {
        ResetPanelPosition();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isFocused = true;

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        animateCoroutine = StartCoroutine(ShiftPanelUp());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isFocused = false;

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        animateCoroutine = StartCoroutine(ResetPanelDown());
    }

    private IEnumerator ShiftPanelUp()
    {
        if (panelToMove == null) yield break;

        // Poll for up to 0.5s until Android finishes opening the keyboard frame
        float timeout = 0.5f;
        float elapsed = 0f;
        float keyboardHeightPixels = 0f;

        while (elapsed < timeout)
        {
            keyboardHeightPixels = GetNativeKeyboardHeight();
            if (keyboardHeightPixels > 0f) break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        float targetShiftY = CalculateRequiredShift(keyboardHeightPixels);
        Vector2 targetPos = originalPanelPos + new Vector2(0f, targetShiftY);

        while (isFocused)
        {
            panelToMove.anchoredPosition = Vector2.Lerp(
                panelToMove.anchoredPosition,
                targetPos,
                Time.deltaTime * lerpSpeed
            );

            yield return null;
        }

        yield return ResetPanelDown();
    }

    private IEnumerator ResetPanelDown()
    {
        if (panelToMove == null) yield break;

        while (Vector2.Distance(panelToMove.anchoredPosition, originalPanelPos) > 0.5f)
        {
            panelToMove.anchoredPosition = Vector2.Lerp(
                panelToMove.anchoredPosition,
                originalPanelPos,
                Time.deltaTime * lerpSpeed
            );

            yield return null;
        }

        panelToMove.anchoredPosition = originalPanelPos;
    }

    /// <summary>
    /// Gets real soft-keyboard height via Android JNI from getWindowVisibleDisplayFrame
    /// </summary>
    private float GetNativeKeyboardHeight()
    {
#if UNITY_EDITOR
        return Screen.height * fallbackScreenPercentage;
#elif UNITY_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            using (var mUnityPlayer = currentActivity.Get<AndroidJavaObject>("mUnityPlayer"))
            {
                using (var view = mUnityPlayer.Call<AndroidJavaObject>("getView"))
                {
                    using (var rect = new AndroidJavaObject("android.graphics.Rect"))
                    {
                        view.Call("getWindowVisibleDisplayFrame", rect);
                        int visibleFrameHeight = rect.Call<int>("height");
                        
                        // Screen height minus visible height = keyboard height in physical pixels
                        int keyboardHeight = Screen.height - visibleFrameHeight;
                        return Mathf.Max(0, keyboardHeight);
                    }
                }
            }
        }
#elif UNITY_IOS
        return TouchScreenKeyboard.area.height;
#else
        return Screen.height * fallbackScreenPercentage;
#endif
    }

    private float CalculateRequiredShift(float keyboardHeightPixels)
    {
        if (keyboardHeightPixels <= 0)
        {
            keyboardHeightPixels = Screen.height * fallbackScreenPercentage;
        }

        Canvas canvas = panelToMove.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

        // Convert Native Keyboard Top Y to Canvas space
        Vector2 keyboardTopCanvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            new Vector2(0, keyboardHeightPixels),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out keyboardTopCanvasPos
        );

        // Convert Input Bottom-Left corner to Canvas space
        Vector3[] worldCorners = new Vector3[4];
        inputRect.GetWorldCorners(worldCorners);

        Vector2 inputBottomCanvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            worldCorners[0], 
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out inputBottomCanvasPos
        );

        // Shift calculation
        float overlapY = (keyboardTopCanvasPos.y + marginAboveKeyboard) - inputBottomCanvasPos.y;

        return overlapY > 0 ? overlapY : 0f;
    }

    public void ResetPanelPosition()
    {
        isFocused = false;

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        if (panelToMove != null)
        {
            panelToMove.anchoredPosition = originalPanelPos;
        }
    }
}