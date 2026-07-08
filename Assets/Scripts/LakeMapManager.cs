using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine; // Unity Cinemachine package

public class LakeMapManager : MonoBehaviour
{
    [Header("References")]
    public DuckJumpController duckController;
    public Transform landReturnPoint; // Landing spot on the shore/beach

    [Header("Cinemachine Cameras")]
    public CinemachineCamera followCam;   // Primary camera following the duck (Priority: 10)
    public CinemachineCamera rockZoomCam; // Secondary close-up camera for rocks (Priority: 5)

    [Header("UI Popups")]
    public GameObject previewTitleUI;
    public TMPro.TextMeshProUGUI titleText;
    public GameObject fullCardInfoUI;
    public GameObject zoomOutButton;

    private JumpRock currentlySelectedRock;
    private JumpRock lastClickedRock;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        // Initialize Camera priorities
        if (followCam != null) followCam.Priority = 10;
        if (rockZoomCam != null) rockZoomCam.Priority = 5;

        // Hide UI elements initially
        if (zoomOutButton != null) zoomOutButton.SetActive(false);
        if (previewTitleUI != null) previewTitleUI.SetActive(false);
        if (fullCardInfoUI != null) fullCardInfoUI.SetActive(false);
    }

    private void Update()
    {
        // Detect player clicks on rocks when the duck is stationary
        if (Input.GetMouseButtonDown(0) && duckController != null && !duckController.IsJumping)
        {
            HandleRockClick();
        }
    }

    private void HandleRockClick()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, duckController.rockLayer))
        {
            JumpRock clickedRock = hit.collider.GetComponent<JumpRock>();
            if (clickedRock == null) return;

            // 1. Rock with 100% Fog -> Direct Jump + Full Info Popup
            if (clickedRock.fogPercentage >= 100f)
            {
                TriggerJumpAndPopup(clickedRock);
            }
            // 2. Rock with < 100% Fog (Partially or Fully Cleared)
            else
            {
                // First Click: Display small Title Preview UI
                if (lastClickedRock != clickedRock)
                {
                    ShowTitlePreview(clickedRock);
                    lastClickedRock = clickedRock;
                }
                // Second Click on same rock: Trigger Jump + Close-up + Full Info
                else
                {
                    HideTitlePreview();
                    TriggerJumpAndPopup(clickedRock);
                }
            }
        }
    }

    private void ShowTitlePreview(JumpRock rock)
    {
        if (previewTitleUI != null)
        {
            if (titleText != null) titleText.text = rock.rockTitle;
            
            // Position the preview slightly above the target rock
            previewTitleUI.transform.position = rock.landingPoint.position + Vector3.up * 3f;
            previewTitleUI.SetActive(true);
        }
    }

    private void HideTitlePreview()
    {
        if (previewTitleUI != null) previewTitleUI.SetActive(false);
    }

    private void TriggerJumpAndPopup(JumpRock rock)
    {
        currentlySelectedRock = rock;
        lastClickedRock = null;

        // Command duck to jump to the rock's landing point
        duckController.JumpTo(rock.landingPoint.position, () =>
        {
            // Callback executing once jump completes:
            
            // A. Align close-up Cinemachine camera with the rock's camera view point
            if (rock.cameraZoomPoint != null && rockZoomCam != null)
            {
                rockZoomCam.transform.position = rock.cameraZoomPoint.position;
                rockZoomCam.transform.rotation = rock.cameraZoomPoint.rotation;

                // Increase priority so Cinemachine smoothly blends to the close-up camera
                rockZoomCam.Priority = 20;
                followCam.Priority = 10;
            }

            // B. Show Info Card & Zoom Out Button
            if (fullCardInfoUI != null) fullCardInfoUI.SetActive(true);
            if (zoomOutButton != null) zoomOutButton.SetActive(true);
        });
    }

    /// <summary>
    /// UI Event: Attach this function to your "Zoom Out" UI Button OnClick() event
    /// </summary>
    public void OnClick_ZoomOutToLand()
    {
        // 1. Hide Popups
        if (fullCardInfoUI != null) fullCardInfoUI.SetActive(false);
        if (previewTitleUI != null) previewTitleUI.SetActive(false);

        // 2. Command duck to jump back to the shore landing point
        if (landReturnPoint != null)
        {
            duckController.JumpTo(landReturnPoint.position, () =>
            {
                // Callback executing once returned to shore:
                // Revert Cinemachine camera priorities back to follow camera
                if (rockZoomCam != null) rockZoomCam.Priority = 5;
                if (followCam != null) followCam.Priority = 10;

                if (zoomOutButton != null) zoomOutButton.SetActive(false);
            });
        }
    }
}