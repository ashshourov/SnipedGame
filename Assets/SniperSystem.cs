using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class SniperSystem : MonoBehaviour
{
    private ARFaceManager faceManager;

    // Timer for the 2-second capture
    private float snipeTimer = 0f;
    private const float SNIPE_DURATION = 2.0f;

    // This will hold the face we are currently targeting
    private ARFace targetFace;

    void Start()
    {
        faceManager = GetComponent<ARFaceManager>();
    }

    void Update()
    {
        // 1. Check if we are detecting any faces
        if (faceManager.trackables.count > 0)
        {
            // 2. Find the face closest to the center of the screen
            targetFace = GetFaceClosestToCenter();

            if (targetFace != null)
            {
                // 3. If a face is targeted, start the timer
                snipeTimer += Time.deltaTime;

                if (snipeTimer >= SNIPE_DURATION)
                {
                    // 4. SNIPE SUCCESSFUL!
                    Debug.Log("SNIPE SUCCESSFUL! Target acquired.");
                    // We will add backend logic here later.

                    // Reset timer to prevent instant re-snipe
                    snipeTimer = 0f;
                }
            } // <-- THIS WAS THE MISSING BRACE
            else
            {
                // No face is in the crosshair, reset timer
                snipeTimer = 0f;
            }
        }
        else
        {
            // No faces detected at all, reset timer
            snipeTimer = 0f;
        }
    }

    private ARFace GetFaceClosestToCenter()
    {
        float minDistance = float.MaxValue;
        ARFace closestFace = null;

        // Get screen center
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        foreach (ARFace face in faceManager.trackables)
        {
            // Get the 2D screen position of the face
            Vector2 facePosition = Camera.main.WorldToScreenPoint(face.transform.position);

            // Check distance from center
            float distance = Vector2.Distance(facePosition, screenCenter);

            // We also need a "threshold" to act as the crosshair
            // If distance is < 200 pixels, it's not in the crosshair
            if (distance < minDistance && distance < 200f)
            {
                minDistance = distance;
                closestFace = face;
            }
        }

        return closestFace; // This will be null if no face is in the crosshair
    }
}