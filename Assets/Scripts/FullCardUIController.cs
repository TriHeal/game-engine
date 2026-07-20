using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class FeelingOption
{
    public string feelingName;
    public Color feelingColor;
}

public class FullCardUIController : MonoBehaviour
{
    [Header("UI Input References")]
    public TMP_InputField titleInput;       // Title ("מה קרה?")
    public TMP_InputField whereInput;       // Where ("איפה זה היה?")
    public TMP_Dropdown feelingDropdown;    // Feeling ("מה הרגשתי?")
    public TMP_InputField thinkingInput;    // Thinking ("מה חשבתי?")
    public TMP_InputField bodyFeelingInput; // BodyFeeling ("מה הרגשתי בגוף?")
    public TMP_InputField goalInput;        // Goal ("הבנתי משהו?")

    [Header("Visual Tinting")]
    public Image dropdownBackgroundImage;   // Optional: Background image of dropdown to color-code it

    [Header("Feelings Configuration")]
    public List<FeelingOption> feelingsList = new List<FeelingOption>()
    {
        new FeelingOption { feelingName = "בחר רגש...", feelingColor = Color.white },
        
        // Original Emotions
        new FeelingOption { feelingName = "שמחה", feelingColor = new Color(1.0f, 0.85f, 0.2f) },   // Golden Yellow
        new FeelingOption { feelingName = "פחד", feelingColor = new Color(0.6f, 0.3f, 0.8f) },     // Violet
        new FeelingOption { feelingName = "כעס", feelingColor = new Color(0.9f, 0.3f, 0.3f) },     // Red
        new FeelingOption { feelingName = "עצב", feelingColor = new Color(0.3f, 0.3f, 0.9f) },     // Blue
        
        // New Additions
        new FeelingOption { feelingName = "בושה", feelingColor = new Color(0.44f, 0.07f, 0.21f) },  // Deep Plum
        new FeelingOption { feelingName = "אשמה", feelingColor = new Color(0.40f, 0.33f, 0.23f) },  // Mud Brown
        new FeelingOption { feelingName = "חוסר אונים", feelingColor = new Color(0.44f, 0.50f, 0.56f) }, // Slate Grey
        new FeelingOption { feelingName = "אימה", feelingColor = new Color(0.10f, 0.07f, 0.15f) },  // Near-Black Ink
        new FeelingOption { feelingName = "בהלה", feelingColor = new Color(1.0f, 0.33f, 0.0f) },   // Electric Orange
        new FeelingOption { feelingName = "חשק", feelingColor = new Color(0.90f, 0.0f, 0.48f) },   // Bright Magenta
        new FeelingOption { feelingName = "ציפייה", feelingColor = new Color(1.0f, 0.60f, 0.0f) }, // Warm Amber
        new FeelingOption { feelingName = "הקלה", feelingColor = new Color(0.31f, 0.89f, 0.65f) }   // Soft Mint
    };

    private JumpRock currentRock;

    private void Start()
    {
        SetupFeelingDropdownOptions();
    }

    private void SetupFeelingDropdownOptions()
    {
        if (feelingDropdown == null) return;

        feelingDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (var item in feelingsList)
        {
            options.Add(item.feelingName);
        }

        feelingDropdown.AddOptions(options);

        feelingDropdown.onValueChanged.RemoveAllListeners();
        feelingDropdown.onValueChanged.AddListener(OnFeelingSelected);
    }

    /// <summary>
    /// Opens the card and loads saved text from the active rock
    /// </summary>
    public void OpenCardForRock(JumpRock rock)
    {
        currentRock = rock;
        gameObject.SetActive(true);

        // Load existing rock story data into UI fields
        if (titleInput != null) titleInput.text = rock.rockTitle;
        if (whereInput != null) whereInput.text = rock.storyData.whereText;
        if (thinkingInput != null) thinkingInput.text = rock.storyData.thinkingText;
        if (bodyFeelingInput != null) bodyFeelingInput.text = rock.storyData.bodyFeelingText;
        if (goalInput != null) goalInput.text = rock.storyData.goalText;

        // Load selected feeling dropdown value
        if (feelingDropdown != null)
        {
            int savedIndex = feelingsList.FindIndex(f => f.feelingName == rock.storyData.feelingText);
            feelingDropdown.value = savedIndex >= 0 ? savedIndex : 0;
            OnFeelingSelected(feelingDropdown.value);
        }
    }

    private void OnFeelingSelected(int index)
    {
        if (index >= 0 && index < feelingsList.Count)
        {
            Color selectedColor = feelingsList[index].feelingColor;

            if (dropdownBackgroundImage != null)
                dropdownBackgroundImage.color = selectedColor;

            // Instantly change 3D rock color if a valid emotion is picked
            if (currentRock != null && index > 0)
            {
                currentRock.SetRockColor(selectedColor);
            }
        }
    }

    /// <summary>
    /// Saves all typed text and dropdown values back to rock data, then reduces fog
    /// </summary>
    public void SaveAndClose()
    {
        if (currentRock != null)
        {
            if (titleInput != null) currentRock.rockTitle = titleInput.text;
            if (whereInput != null) currentRock.storyData.whereText = whereInput.text;
            if (thinkingInput != null) currentRock.storyData.thinkingText = thinkingInput.text;
            if (bodyFeelingInput != null) currentRock.storyData.bodyFeelingText = bodyFeelingInput.text;
            if (goalInput != null) currentRock.storyData.goalText = goalInput.text;

            // Save the selected feeling string
            if (feelingDropdown != null)
                currentRock.storyData.feelingText = feelingsList[feelingDropdown.value].feelingName;

            // Reduce/clear fog dynamically based on completed info!
            float newFog = CalculateFogLevel();
            currentRock.UpdateFogLevel(newFog);
        }

        gameObject.SetActive(false);
    }

    private float CalculateFogLevel()
    {
        bool hasTitle = titleInput != null && !string.IsNullOrWhiteSpace(titleInput.text);
        bool hasWhere = whereInput != null && !string.IsNullOrWhiteSpace(whereInput.text);
        bool hasFeeling = feelingDropdown != null && feelingDropdown.value > 0;
        bool hasThinking = thinkingInput != null && !string.IsNullOrWhiteSpace(thinkingInput.text);
        bool hasBodyFeeling = bodyFeelingInput != null && !string.IsNullOrWhiteSpace(bodyFeelingInput.text);
        bool hasGoal = goalInput != null && !string.IsNullOrWhiteSpace(goalInput.text);

        // Special rule: if we have feeling + goal, it can go all the way down to 0 right away
        if (hasFeeling && hasGoal)
        {
            return 0f;
        }

        // Standard rule: fog goes down with the more info we have (based on 6 total fields)
        int completedCount = 0;
        if (hasTitle) completedCount++;
        if (hasWhere) completedCount++;
        if (hasFeeling) completedCount++;
        if (hasThinking) completedCount++;
        if (hasBodyFeeling) completedCount++;
        if (hasGoal) completedCount++;

        return 100f * (1f - (float)completedCount / 6f);
    }
}