using UnityEngine;

public class ForestPlantingManager : MonoBehaviour
{
    [Header("Planting Settings")]
    public GameObject[] treePrefabs;          
    public LayerMask forestGroundLayer;       
    public Transform treesContainer;          

    [Header("Tree Spawn Tweaks")]
    public bool randomRotation = true;        
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f); 

    [Header("Feeling UI Connections")]
    public GameObject feelingPopupUI;         // Drag your ForestFeelingPopup panel here

    private Camera mainCam;
    private Vector3 cachedSpawnPoint;          // Stores where the kid clicked
    private bool isWaitingForFeeling = false;
    private Vector2 touchStartPos;
    private const float maxTapDistance = 15f; // Threshold in pixels to distinguish tap vs swipe

    private void Start()
    {
        mainCam = Camera.main;
        if (feelingPopupUI != null) feelingPopupUI.SetActive(false);
    }

    private void Update()
    {
        if (isWaitingForFeeling) return;

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Only trigger tree planting if finger didn't move far (i.e. it was a tap, not a swipe)
            if (Vector2.Distance(touchStartPos, Input.mousePosition) < maxTapDistance)
            {
                TryOpenPlantingMenu(Input.mousePosition);
            }
        }
    }

    private void TryOpenPlantingMenu(Vector3 screenPosition)
    {
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        Ray ray = mainCam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, forestGroundLayer))
        {
            // 1. Save the ground click position
            cachedSpawnPoint = hit.point;
            isWaitingForFeeling = true;

            // 2. Open the emotion buttons panel
            if (feelingPopupUI != null)
            {
                feelingPopupUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Attach this method directly to each color button's OnClick() in Unity!
    /// Pass a specific hex color string (e.g., "#FFD700" for Joy, "#8A2BE2" for Fear).
    /// </summary>
    public void SelectFeelingAndPlant(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color selectedColor))
        {
            PlantTreeWithColor(selectedColor);
        }
        else
        {
            Debug.LogWarning($"ForestPlantingManager: Could not parse hex color string: {hexColor}");
            PlantTreeWithColor(Color.white); // Fallback color
        }
    }

    private void PlantTreeWithColor(Color chosenColor)
    {
        // 1. Pick a random tree prefab model
        int randomIndex = Random.Range(0, treePrefabs.Length);
        GameObject selectedTreePrefab = treePrefabs[randomIndex];

        // 2. Determine rotation and random size scale
        Quaternion spawnRotation = randomRotation ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity;
        float randomScale = Random.Range(scaleRange.x, scaleRange.y);

        // 3. Spawn the tree
        GameObject newTree = Instantiate(selectedTreePrefab, cachedSpawnPoint, spawnRotation);
        newTree.transform.localScale = Vector3.one * randomScale;

        if (treesContainer != null) 
        {
            newTree.transform.SetParent(treesContainer);
        }

        // 4. Tint the tree leaves/renderer
        Renderer treeRenderer = newTree.GetComponentInChildren<Renderer>();
        if (treeRenderer != null)
        {
            treeRenderer.material.color = chosenColor;
        }

        // 5. Close popup UI and resume ground clicking
        if (feelingPopupUI != null) feelingPopupUI.SetActive(false);
        isWaitingForFeeling = false;
    }
}