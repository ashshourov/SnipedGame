using UnityEngine;
using UnityEngine.UI; // Required for RawImage
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Unity; // Necessary for accessing MediaPipe types

// This script now acts as the central controller for the dual MediaPipe scene.
public class MediaPipeFaceDetector : MonoBehaviour
{
    // --- Data Fields ---
    [Header("Camera Components")]
    public RawImage cameraDisplay;
    // Must be public and manually assigned in the Inspector, as WebCamSource is not a MonoBehaviour.
    [SerializeField] public WebCamSource webCamSource;
    private WebCamTexture webCamTexture;

    [Header("Snipe Settings")]
    private const float SNIPE_DURATION = 2.0f; // 2-second capture
    private float snipeTimer = 0f;
    private bool isTrackingTarget = false;
    private bool isCameraReady = false; // Internal flag for camera status
    
    // Thread-safe state management
    private object detectionLock = new object();

    [Header("Manager References")]
    public PopupManager popupManager;
    private TargetSelectionUI targetSelectionUI;
    private BlockSystem blockSystem; // Local reference to the BlockSystem

    // --- Initialization ---
    void Start()
    {
        // Safety check: Find the TargetSelectionUI Instance
        targetSelectionUI = TargetSelectionUI.Instance;

        // Find the BlockSystem instance in the scene
        blockSystem = FindObjectOfType<BlockSystem>();

        // Ensure critical component is linked before starting camera
        if (webCamSource == null)
        {
            Debug.LogError("WebCamSource is NOT assigned in the Inspector! Camera will not start.");
            return;
        }

        // Safety check: Find the popup manager if not explicitly assigned
        if (popupManager == null)
        {
            popupManager = FindObjectOfType<PopupManager>();
        }

        // Start the camera and set the texture
        StartCoroutine(StartCameraRoutine());

        Debug.Log("SniperSystem initialized and awaiting camera.");
    }

    IEnumerator StartCameraRoutine()
    {
        // 1. Start the camera and wait for it to initialize (Play() handles permissions)
        yield return webCamSource.Play();

        // 2. Check if the camera actually started
        webCamTexture = webCamSource.GetCurrentTexture() as WebCamTexture;

        if (webCamTexture != null)
        {
            if (cameraDisplay != null)
            {
                // Set the display texture
                cameraDisplay.texture = webCamTexture;
            }
            isCameraReady = true; // Set flag to true
            Debug.Log("Camera texture set and ready.");
        }
        else
        {
            Debug.LogError("CRITICAL: WebCamTexture failed to start. Check permissions/device.");
        }
    }

    void Update()
    {
        // Wait for camera initialization and PlayFab authentication
        if (!isCameraReady || PlayFabManager.Instance == null || !PlayFabManager.Instance.IsAuthenticated)
        {
            return;
        }

        // Thread-safe timer update only if tracking
        lock (detectionLock)
        {
            if (isTrackingTarget)
            {
                snipeTimer += Time.deltaTime;

                if (snipeTimer >= SNIPE_DURATION)
                {
                    OnSnipeSuccess();
                    snipeTimer = 0f;
                    isTrackingTarget = false; // Stop tracking after success/fail
                }
            }
        }
    }

    // --- MediaPipe Link Function (Called by the Face Detection Runner) ---
    // Thread-safe callback from MediaPipe detection event
    public void OnFaceDetectionEvent(bool faceIsPresent)
    {
        lock (detectionLock)
        {
            // Only transition from no-tracking to tracking, not vice-versa in this call
            if (faceIsPresent && !isTrackingTarget)
            {
                isTrackingTarget = true;
                Debug.Log("Face detected - starting snipe timer.");
            }
            else if (!faceIsPresent && isTrackingTarget)
            {
                // Face lost during tracking - reset timer
                snipeTimer = 0f;
                isTrackingTarget = false;
                Debug.Log("Face lost - snipe timer reset.");
            }
        }
    }

    void OnSnipeSuccess()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("SNIPE SUCCESSFUL! Triggering UI.");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        // Check Block status (Block gesture prevents capture)
        // Use the local blockSystem reference for the static flag check
        bool isBlocked = (blockSystem != null) && BlockSystem.isBlocking;

        if (!isBlocked)
        {
            // Show the team list for identity confirmation
            if (targetSelectionUI != null)
            {
                targetSelectionUI.DisplayTeamList();
            }
            else
            {
                Debug.LogError("TargetSelectionUI missing! Cannot display team list.");
            }
        }
        else
        {
            Debug.Log("SNIPE BLOCKED! Target defense active.");
        }

        isTrackingTarget = false; // Prevent immediate re-snipe
    }
}