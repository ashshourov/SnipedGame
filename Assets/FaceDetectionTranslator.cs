using UnityEngine;
using Mediapipe.Tasks.Components.Containers; // For DetectionResult
using Mediapipe.Unity.Sample.FaceDetection; // To find the Runner
using System; // For OnDestroy cleanup

public class FaceDetectionTranslator : MonoBehaviour
{
    // Assign the Runner in the Inspector
    public FaceDetectorRunner faceRunner;

    // Assign the final game logic script here
    // NOTE: This must be linked manually to the MediaPipeFaceDetector script component.
    public MediaPipeFaceDetector gameController;

    void Start()
    {
        //search for the game controller in the scene
        gameController = FindObjectOfType<MediaPipeFaceDetector>();

        // Safety check: Subscribe to the public event exposed in the Runner script
        if (faceRunner != null)
        {
            // The event is subscribed here, calling ProcessDetectionOutput on the background thread.
            faceRunner.OnFaceDetectionResult += ProcessDetectionOutput;
            Debug.Log("Translator: Subscribed to Face Runner output.");
        }
        
    }

    // This method runs on the MediaPipe background thread when a result is ready.
    private void ProcessDetectionOutput(DetectionResult result)
    {
        // CRITICAL CHECK: Ensure the controller is linked. If null, we cannot proceed.
        // We do NOT use FindObjectOfType here to avoid a crash.
        if (gameController == null)
        {
            Debug.LogError("Translator Error: Game Controller is missing. Cannot process face data.");
            return;
        }

        // 1. Determine if a face is present.
        // Check for the presence of Detections list and if it contains data.
        bool faceIsPresent = result.detections != null && result.detections.Count > 0;

        // 2. Call the final game logic function (dispatched to the game controller)
        gameController.OnFaceDetectionEvent(faceIsPresent);
    }

    void OnDestroy()
    {
        // Clean up the subscription when the object is destroyed (critical for performance)
        if (faceRunner != null)
        {
            faceRunner.OnFaceDetectionResult -= ProcessDetectionOutput;
        }
    }
}