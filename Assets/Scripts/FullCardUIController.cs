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
        new FeelingOption { feelingName = "בחר הרגשה...", feelingColor = Color.white },
        new FeelingOption { feelingName = "פחד", feelingColor = new Color(0.6f, 0.3f, 0.8f) },
        new FeelingOption { feelingName = "שמחה", feelingColor = new Color(1f, 0.85f, 0.2f) },
        new FeelingOption { feelingName = "כעס", feelingColor = new Color(0.9f, 0.3f, 0.3f) },
        new FeelingOption { feelingName = "עצב", feelingColor = new Color(0.3f, 0.6f, 0.9f) }
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

            // Reduce/clear fog on completion!
            currentRock.UpdateFogLevel(0f);
        }

        gameObject.SetActive(false);
    }
}