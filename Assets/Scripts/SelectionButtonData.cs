using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SelectionButtonData : MonoBehaviour
{
    [Header("Option Configuration")]
    public string optionId;         // e.g., "joy", "fear"
    public string hexColor;         // e.g., "#FFD000"
    
    [TextArea(2, 3)]
    public string promptText;       // e.g., "ספרו על משהו משמח שקרה לכם היום!"

    private Button button;
    private ForestPlantingManager manager;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Find the manager in the scene automatically
        manager = Object.FindFirstObjectByType<ForestPlantingManager>();

        if (button != null && manager != null)
        {
            // Automatically registers its own click event
            button.onClick.AddListener(NotifyManagerOfSelection);
        }
    }

    private void NotifyManagerOfSelection()
    {
        if (manager != null)
        {
            manager.SelectOption(this);
        }
    }
}