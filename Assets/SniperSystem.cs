using UnityEngine;
using UnityEngine.XR.ARFoundation;

public enum PlayerState
{
    ACTIVE,
    RESPAWNING
}

public class SniperSystem : MonoBehaviour
{
    [Header("Player Status")]
    public PlayerState currentState = PlayerState.ACTIVE;

    private const float RESPAWN_DURATION = 20.0f;
    private float respawnTimer = 0f;

    [Header("Snipe Logic")]
    private const float SNIPE_DURATION = 2.0f;
    private float snipeTimer = 0f;

    [Header("AR Components")]
    private ARFaceManager faceManager;

    [Header("Manager References")]
    public PopupManager popupManager;

    void Start()
    {
        // Get AR components (Unity 2023+ uses FindFirstObjectByType)
        faceManager = FindFirstObjectByType<ARFaceManager>();
        if (faceManager == null)
        {
            Debug.LogError("ARFaceManager not found in scene! AR functionality disabled.");
        }

        // Find PopupManager if not assigned
        if (popupManager == null)
        {
            popupManager = FindFirstObjectByType<PopupManager>();
            if (popupManager == null)
            {
                Debug.LogWarning("PopupManager not found. Popups will only show in Logcat.");
            }
        }

        Debug.Log("SniperSystem initialized. Current state: " + currentState);
    }

    void Update()
    {
        // Check if player is authenticated with PlayFab
        if (PlayFabManager.Instance != null && PlayFabManager.Instance.IsLoggedIn())
        {
            switch (currentState)
            {
                case PlayerState.ACTIVE:
                    HandleActiveState();
                    break;
                case PlayerState.RESPAWNING:
                    HandleRespawningState();
                    break;
            }
        }
        else
        {
            // Waiting for authentication
            // Don't spam logs - only log once per second
            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("Waiting for user authentication to activate game logic.");
            }
        }

        // Debug test code (ONLY in Editor)
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key pressed - Testing TargetSelectionUI");
            TargetSelectionUI uiInstance = TargetSelectionUI.Instance;
            if (uiInstance != null)
            {
                uiInstance.DisplayTeamList();
            }
            else
            {
                Debug.LogError("Cannot test UI: TargetSelectionUI.Instance is NULL!");
            }
        }
#endif
    }

    void HandleActiveState()
    {
        if (faceManager == null) return;

        // Check if we are detecting any faces
        if (faceManager.trackables.count == 0)
        {
            if (snipeTimer > 0)
            {
                Debug.Log("Face lost. Resetting snipe timer.");
                snipeTimer = 0f;
            }
            return;
        }

        // Find a detected face
        ARFace targetFace = GetFirstDetectedFace();

        if (targetFace != null)
        {
            // Face is detected, increment the timer
            snipeTimer += Time.deltaTime;

            // Log progress for debugging (every 0.5 seconds)
            if (snipeTimer % 0.5f < Time.deltaTime)
            {
                Debug.Log($"Tracking face... {snipeTimer:F1}s / {SNIPE_DURATION}s");
            }

            // Check if timer reached the snipe duration
            if (snipeTimer >= SNIPE_DURATION)
            {
                // Check if target is blocking
                if (BlockSystem.isBlocking == false)
                {
                    Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    Debug.Log("SNIPE SUCCESSFUL! Target confirmed via detection.");
                    Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                    // Show success popup
                    if (popupManager != null)
                    {
                        Debug.Log("Calling ShowSuccessPopup...");
                        popupManager.ShowSuccessPopup();
                    }
                    else
                    {
                        Debug.LogError("PopupManager is null! Cannot show popup.");
                    }

                    // Display target selection UI
                    Debug.Log("Attempting to show TargetSelectionUI...");
                    TargetSelectionUI uiInstance = TargetSelectionUI.Instance;
                    if (uiInstance != null)
                    {
                        Debug.Log("TargetSelectionUI found! Calling DisplayTeamList...");
                        uiInstance.DisplayTeamList();
                    }
                    else
                    {
                        Debug.LogError("UI FAILURE: TargetSelectionUI Instance is NULL!");
                        Debug.LogError("Make sure TargetSelectionUI script is in the scene and active!");
                    }
                }
                else
                {
                    Debug.Log("SNIPE BLOCKED! Target was blocking.");
                }

                // Reset the timer after a snipe attempt
                snipeTimer = 0f;
            }
        }
        else
        {
            // Face was lost
            snipeTimer = 0f;
        }
    }

    void HandleRespawningState()
    {
        respawnTimer -= Time.deltaTime;

        // Log every second
        if (Mathf.FloorToInt(respawnTimer) != Mathf.FloorToInt(respawnTimer + Time.deltaTime))
        {
            Debug.Log($"Respawning... {Mathf.CeilToInt(respawnTimer)}s left");
        }

        if (respawnTimer <= 0)
        {
            currentState = PlayerState.ACTIVE;
            respawnTimer = 0f;
            snipeTimer = 0f;
            Debug.Log("Respawn complete! You are ACTIVE again.");
        }
    }

    // Called by StatusListener to start the timer
    public void StartRespawnTimer(float duration)
    {
        if (currentState == PlayerState.ACTIVE)
        {
            currentState = PlayerState.RESPAWNING;
            respawnTimer = duration;
            snipeTimer = 0f;
            Debug.Log($"SERVER NOTIFICATION: YOU WERE SNIPED! Respawning for {duration:F1} seconds.");
        }
    }

    // Debug function
    public void GotSniped()
    {
        if (currentState == PlayerState.ACTIVE)
        {
            currentState = PlayerState.RESPAWNING;
            respawnTimer = RESPAWN_DURATION;
            snipeTimer = 0f;
            Debug.Log($"YOU WERE SNIPED! Respawning for {RESPAWN_DURATION} seconds.");
        }
    }

    // Helper function
    private ARFace GetFirstDetectedFace()
    {
        if (faceManager != null && faceManager.trackables.count > 0)
        {
            foreach (ARFace face in faceManager.trackables)
            {
                return face;
            }
        }
        return null;
    }
}