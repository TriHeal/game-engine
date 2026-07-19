using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ForestPlantingManager : MonoBehaviour
{
    [Header("Planting Settings")]
    public GameObject[] treePrefabs;          
    public LayerMask forestGroundLayer;       
    public Transform treesContainer;          

    [Header("Tree Spawn Tweaks")]
    public bool randomRotation = true;        
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f); 

    [Header("UI Control Connections")]
    public GameObject optionPopupUI;         
    public GameObject openMenuHUDButton; // The top-corner button to disable/enable
    public TMP_Text popupInstructionText; // Your new TextMeshPro - Text inside the popup
    public Button popupConfirmButton;     // The confirm button inside the popup

    [Header("Dynamic Prompt Defaults")]
    public string defaultPrompt = "בחרו אפשרות מתוך הרשימה:";

    private Camera mainCam;
    private bool hasConfirmedSelection = false; 
    private Color storedColor = Color.white;    
    
    private Image confirmButtonImage;
    private Color originalConfirmColor;

    private Vector2 touchStartPos;
    private const float maxTapDistance = 15f;   

    private void Start()
    {
        mainCam = Camera.main;
        
        if (optionPopupUI != null) optionPopupUI.SetActive(false);
        if (openMenuHUDButton != null) openMenuHUDButton.SetActive(true);
        
        // Cache the original confirm button color so we can reset it later
        if (popupConfirmButton != null)
        {
            confirmButtonImage = popupConfirmButton.GetComponent<Image>();
            if (confirmButtonImage != null) originalConfirmColor = confirmButtonImage.color;
        }

        hasConfirmedSelection = false;
    }

    private void Update()
    {
        // If they haven't confirmed their UI choice yet, stop.
        if (!hasConfirmedSelection) return;
        
        // Safety check: if the popup is still somehow active, stop.
        if (optionPopupUI != null && optionPopupUI.activeSelf) return;

        // Detect the start of the interaction (Mouse Click or Finger Touch)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if clicking over a UI element (Mobile vs PC safe)
            if (IsPointerOverUI())
            {
                touchStartPos = new Vector2(-9999f, -9999f); // Invalidate the position
                return; 
            }

            touchStartPos = Input.mousePosition;
        }

        // Detect the release of the interaction
        if (Input.GetMouseButtonUp(0))
        {
            // Double safety check on release: ensure they aren't releasing over UI
            if (IsPointerOverUI())
            {
                return;
            }

            if (Vector2.Distance(touchStartPos, Input.mousePosition) < maxTapDistance)
            {
                TryPlantTreeAtTap(Input.mousePosition);
            }
        }
    }

    /// <summary>
    /// Helper function that reliably returns true if the user is tapping a UI element
    /// on either PC, Mac, Android, or iOS.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Mobile touch handling
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }
        
        // Standalone desktop/editor mouse handling
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void OpenSelectionMenu()
    {
        if (optionPopupUI != null) 
        {
            optionPopupUI.SetActive(true);
            
            // 1. Request 1: Disable the upper-corner button when popup is enabled
            if (openMenuHUDButton != null) openMenuHUDButton.SetActive(false);

            // Reset popup texts and colors to default upon opening
            if (popupInstructionText != null) popupInstructionText.text = defaultPrompt;
            if (confirmButtonImage != null) confirmButtonImage.color = originalConfirmColor;
        }
    }

    /// <summary>
    /// Receives the full data component package directly from the pressed button.
    /// </summary>
    public void SelectOption(SelectionButtonData clickedButtonData)
    {
        if (clickedButtonData == null) return;

        // Parse color
        if (!ColorUtility.TryParseHtmlString(clickedButtonData.hexColor, out storedColor))
        {
            storedColor = Color.white;
        }

        // 2. Request 2: Change the confirm button color to reflect the choice
        if (confirmButtonImage != null) confirmButtonImage.color = storedColor;

        // 3. Request 3: Change TextMeshPro text based on the chosen option
        if (popupInstructionText != null) popupInstructionText.text = clickedButtonData.promptText;
    }

    public void ConfirmSelection()
    {
        hasConfirmedSelection = true;
        if (optionPopupUI != null) optionPopupUI.SetActive(false);
        
        // Keep the corner HUD button hidden while they place the tree
        if (openMenuHUDButton != null) openMenuHUDButton.SetActive(false);
    }

    private void TryPlantTreeAtTap(Vector3 screenPosition)
    {
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        Ray ray = mainCam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, forestGroundLayer))
        {
            PlantTreeWithColor(hit.point, storedColor);
        }
    }

    private void PlantTreeWithColor(Vector3 spawnPoint, Color chosenColor)
    {
        int randomIndex = Random.Range(0, treePrefabs.Length);
        GameObject selectedTreePrefab = treePrefabs[randomIndex];

        Quaternion spawnRotation = randomRotation ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity;
        float randomScale = Random.Range(scaleRange.x, scaleRange.y);

        GameObject newTree = Instantiate(selectedTreePrefab, spawnPoint, spawnRotation);
        newTree.transform.localScale = Vector3.one * randomScale;
        if (treesContainer != null) newTree.transform.SetParent(treesContainer);

        Renderer treeRenderer = newTree.GetComponentInChildren<Renderer>();
        if (treeRenderer != null) treeRenderer.material.color = chosenColor;

        hasConfirmedSelection = false;
        
        // Re-enable the upper-corner HUD button once the loop is fully done
        if (openMenuHUDButton != null) openMenuHUDButton.SetActive(true);
    }

    public void GoBack()
    {
        if (optionPopupUI != null && optionPopupUI.activeSelf)
        {
            optionPopupUI.SetActive(false);
            hasConfirmedSelection = false;
            if (openMenuHUDButton != null) openMenuHUDButton.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("HomeScreen");
        }
    }
}