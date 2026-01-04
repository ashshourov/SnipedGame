using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackCameraFaceDetector : MonoBehaviour
{
    [Header("Camera Settings")]
    public RawImage displayImage; // UI RawImage to show camera feed
    private WebCamTexture webCamTexture;
    private int backCameraIndex = -1;

    [Header("Face Detection")]
    public GameObject faceIndicator; // UI element to show face detected
    private bool isFaceDetected = false;
    private float faceDetectionTimer = 0f;
    private const float SNIPE_DURATION = 2.0f;

    [Header("Snipe System")]
    public PopupManager popupManager;

    void Start()
    {
        InitializeBackCamera();
    }

    void InitializeBackCamera()
    {
        // Find back camera
        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log($"Found {devices.Length} camera devices:");

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log($"Camera {i}: {devices[i].name} - FrontFacing: {devices[i].isFrontFacing}");

            // Find the back camera (not front-facing)
            if (!devices[i].isFrontFacing)
            {
                backCameraIndex = i;
                Debug.Log($"Back camera found at index {i}");
                break;
            }
        }

        // If no back camera found, use index 1 as fallback
        if (backCameraIndex == -1)
        {
            backCameraIndex = devices.Length > 1 ? 1 : 0;
            Debug.LogWarning($"Back camera not identified, using index {backCameraIndex}");
        }

        // Start the camera
        StartCamera(backCameraIndex);
    }

    void StartCamera(int deviceIndex)
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No camera devices found!");
            return;
        }

        string deviceName = WebCamTexture.devices[deviceIndex].name;

        // Create WebCamTexture (1920x1080 at 30fps for good quality)
        webCamTexture = new WebCamTexture(deviceName, 1920, 1080, 30);

        // Assign to RawImage
        if (displayImage != null)
        {
            displayImage.texture = webCamTexture;
        }

        // Start the camera
        webCamTexture.Play();

        Debug.Log($"Camera started: {deviceName}");
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
            return;

        // This is a placeholder for face detection
        // You'll replace this with actual MediaPipe face detection
        DetectFacesPlaceholder();

        // Handle snipe timer
        if (isFaceDetected)
        {
            faceDetectionTimer += Time.deltaTime;

            if (faceDetectionTimer >= SNIPE_DURATION)
            {
                OnSnipeSuccessful();
                faceDetectionTimer = 0f;
                isFaceDetected = false;
            }
        }
        else
        {
            faceDetectionTimer = 0f;
        }
    }

    // Placeholder - Replace with actual MediaPipe face detection
    void DetectFacesPlaceholder()
    {
        // For now, press Space to simulate face detection
        if (Input.GetKey(KeyCode.Space))
        {
            isFaceDetected = true;
            if (faceIndicator != null)
                faceIndicator.SetActive(true);

            Debug.Log($"Face detected! Timer: {faceDetectionTimer:F1}s / {SNIPE_DURATION}s");
        }
        else
        {
            isFaceDetected = false;
            if (faceIndicator != null)
                faceIndicator.SetActive(false);
        }
    }

    void OnSnipeSuccessful()
    {
        Debug.Log("SNIPE SUCCESSFUL!");

        if (popupManager != null)
        {
            popupManager.ShowSuccessPopup();
        }

        TargetSelectionUI uiInstance = TargetSelectionUI.Instance;
        if (uiInstance != null)
        {
            uiInstance.DisplayTeamList();
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }

    // Public method to switch cameras if needed
    public void SwitchCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        backCameraIndex = (backCameraIndex + 1) % WebCamTexture.devices.Length;
        StartCamera(backCameraIndex);
    }
}