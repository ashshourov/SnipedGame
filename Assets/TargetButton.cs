using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetButton : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI playerNameText;
    public Button button;

    private string targetPlayerName;

    void Start()
    {
        // Get components if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Add click listener
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("Button component not found on TargetButton!");
        }
    }

    // Set the player data for this button
    public void SetPlayerData(string playerName)
    {
        targetPlayerName = playerName;

        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }
        else
        {
            Debug.LogWarning($"PlayerNameText not assigned for {playerName}");
        }

        Debug.Log($"TargetButton set up for: {playerName}");
    }

    // Called when button is clicked
    private void OnButtonClicked()
    {
        Debug.Log($"Target button clicked: {targetPlayerName}");

        // Get the TargetSelectionUI instance and call the finalize method
        if (TargetSelectionUI.Instance != null)
        {
            TargetSelectionUI.Instance.FinalizeSnipeExecution(targetPlayerName);
        }
        else
        {
            Debug.LogError("TargetSelectionUI Instance not found!");
        }
    }
}