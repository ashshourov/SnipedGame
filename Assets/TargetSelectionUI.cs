using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TargetSelectionUI : MonoBehaviour
{
    // Singleton pattern
    public static TargetSelectionUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject targetSelectionPanel; // The main panel to show/hide
    public Transform contentHolder; // The Content object inside ScrollView
    public GameObject playerButtonPrefab; // Button prefab for each player

    [Header("Debug")]
    public bool debugMode = true;

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            if (debugMode) Debug.Log("✓ TargetSelectionUI Instance created successfully.");
        }
        else
        {
            Debug.LogWarning("⚠ Duplicate TargetSelectionUI found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // Validate references immediately
        ValidateReferences();
    }

    private void Start()
    {
        // Hide panel at start
        if (targetSelectionPanel != null)
        {
            targetSelectionPanel.SetActive(false);
            if (debugMode) Debug.Log("✓ TargetSelectionUI initialized. Panel hidden at start.");
        }
        else
        {
            Debug.LogError("✗ targetSelectionPanel is NULL in Start()!");
        }
    }

    private void ValidateReferences()
    {
        bool allValid = true;

        if (targetSelectionPanel == null)
        {
            Debug.LogError("✗ TARGET SELECTION PANEL is NOT assigned in Inspector!");
            allValid = false;
        }
        else
        {
            if (debugMode) Debug.Log("✓ Target Selection Panel is assigned: " + targetSelectionPanel.name);
        }

        if (contentHolder == null)
        {
            Debug.LogError("✗ CONTENT HOLDER is NOT assigned in Inspector!");
            allValid = false;
        }
        else
        {
            if (debugMode) Debug.Log("✓ Content Holder is assigned: " + contentHolder.name);
        }

        if (playerButtonPrefab == null)
        {
            Debug.LogWarning("⚠ PLAYER BUTTON PREFAB is NOT assigned. Buttons won't be created.");
        }
        else
        {
            if (debugMode) Debug.Log("✓ Player Button Prefab is assigned: " + playerButtonPrefab.name);
        }

        if (!allValid)
        {
            Debug.LogError("✗✗✗ CRITICAL: TargetSelectionUI is missing required references! Check Inspector!");
        }
    }

    // Called by SniperSystem when snipe is successful
    public void DisplayTeamList()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("DisplayTeamList() CALLED!");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        // Final validation check
        if (targetSelectionPanel == null)
        {
            Debug.LogError("✗ Cannot display team list - targetSelectionPanel is NULL!");
            return;
        }

        // Check if panel is already active
        if (targetSelectionPanel.activeSelf)
        {
            Debug.LogWarning("⚠ Panel is already active!");
        }
        else
        {
            Debug.Log("→ Panel is currently inactive. Activating now...");
        }

        // Show the panel
        targetSelectionPanel.SetActive(true);

        // Verify it's actually active
        if (targetSelectionPanel.activeSelf)
        {
            Debug.Log("✓✓✓ TARGET SELECTION PANEL IS NOW ACTIVE! ✓✓✓");
        }
        else
        {
            Debug.LogError("✗✗✗ FAILED TO ACTIVATE PANEL! Check if parent Canvas is active!");
        }

        // Populate team list
        PopulateTeamList();
    }

    private void PopulateTeamList()
    {
        Debug.Log("→ PopulateTeamList() called");

        if (contentHolder == null)
        {
            Debug.LogError("✗ Content Holder is NULL! Cannot populate list.");
            return;
        }

        // Clear existing buttons
        int childCount = contentHolder.childCount;
        Debug.Log($"→ Clearing {childCount} existing children from content holder");

        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(contentHolder.GetChild(i).gameObject);
        }

        // Get team members (TODO: Replace with actual team data from Firebase/GameManager)
        List<string> teamMembers = GetTeamMembers();
        Debug.Log($"→ Creating buttons for {teamMembers.Count} team members");

        if (teamMembers.Count == 0)
        {
            Debug.LogWarning("⚠ No team members found! Add test data or connect to backend.");
        }

        // Create a button for each team member
        foreach (string member in teamMembers)
        {
            CreatePlayerButton(member);
        }

        Debug.Log($"✓ Created {teamMembers.Count} player buttons successfully");
    }

    private List<string> GetTeamMembers()
    {
        // TODO: Get actual team members from your GameManager or Firebase
        // For now, return dummy data for testing
        return new List<string>
        {
            "Player 1",
            "Player 2",
            "Player 3",
            "Player 4",
            "Player 5"
        };
    }

    private void CreatePlayerButton(string playerName)
    {
        if (playerButtonPrefab == null)
        {
            Debug.LogError("✗ Cannot create button - playerButtonPrefab is NULL!");
            return;
        }

        // Instantiate the button
        GameObject buttonObj = Instantiate(playerButtonPrefab, contentHolder);
        buttonObj.name = "PlayerButton_" + playerName;

        // Set the button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = playerName;
        }
        else
        {
            // Try regular Text component if TMP not found
            Text regularText = buttonObj.GetComponentInChildren<Text>();
            if (regularText != null)
            {
                regularText.text = playerName;
            }
            else
            {
                Debug.LogWarning($"⚠ No text component found in button for {playerName}");
            }
        }

        // Add click listener
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnPlayerSelected(playerName));
        }
        else
        {
            Debug.LogError($"✗ No Button component found on prefab for {playerName}!");
        }

        if (debugMode) Debug.Log($"✓ Created button for: {playerName}");
    }

    private void OnPlayerSelected(string playerName)
    {
        Debug.Log($"━━━ PLAYER SELECTED: {playerName} ━━━");

        // Hide the panel
        HideTeamList();

        // TODO: Send snipe data to server
        // FirebaseManager.Instance.SendSnipeConfirmation(playerName);
        Debug.Log($"→ TODO: Send snipe confirmation to server for {playerName}");
    }

    // Called by TargetButton when a target is selected
    public void FinalizeSnipeExecution(string targetName)
    {
        Debug.Log($"━━━ FINALIZE SNIPE EXECUTION: {targetName} ━━━");

        // Hide the panel
        HideTeamList();

        // TODO: Send snipe data to server with target name
        // FirebaseManager.Instance.SendSnipeConfirmation(targetName);
        Debug.Log($"→ Sending snipe confirmation to server for target: {targetName}");

        // TODO: Show confirmation message to user
        Debug.Log($"✓ Snipe confirmed on {targetName}!");
    }

    public void HideTeamList()
    {
        if (targetSelectionPanel != null)
        {
            targetSelectionPanel.SetActive(false);
            Debug.Log("✓ Target selection panel hidden");
        }
    }

    // Manual test function - Press L in editor to test
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("━━━ MANUAL TEST: L key pressed - showing team list ━━━");
            DisplayTeamList();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("━━━ MANUAL TEST: H key pressed - hiding team list ━━━");
            HideTeamList();
        }
#endif
    }

    // Debug function to check status
    public void LogStatus()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("TARGETSELECTIONUI STATUS:");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log($"Panel assigned: {targetSelectionPanel != null}");
        if (targetSelectionPanel != null)
        {
            Debug.Log($"Panel active: {targetSelectionPanel.activeSelf}");
        }
        Debug.Log($"Content Holder assigned: {contentHolder != null}");
        Debug.Log($"Button Prefab assigned: {playerButtonPrefab != null}");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}