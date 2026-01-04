using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Containers = Mediapipe.Tasks.Components.Containers;

public class GestureFilter : MonoBehaviour
{
    public BlockSystem blockSystem;
    public HandLandmarkerRunner handRunner;

    private const float FINGER_OPEN_THRESHOLD = 0.08f; // Increased from 0.04 to be less sensitive
    private bool hasLoggedStructure = false;

    // Cached reflection info for performance
    private PropertyInfo landmarkListProperty;
    private PropertyInfo yCoordinateProperty;
    private object lastLandmarkType;

    void Start()
    {
        if (handRunner != null)
        {
            handRunner.OnHandResult += OnHandLandmarkResult;
            Debug.Log("GestureFilter: Subscribed to HandLandmarker events.");
        }
        else
        {
            Debug.LogError("GestureFilter: Hand Runner is NULL. Cannot link events.");
        }
    }

    private void OnHandLandmarkResult(HandLandmarkerResult result)
    {
        if (blockSystem == null || BlockSystem.isBlocking)
            return;

        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
            return;

        // Log structure once for debugging
        if (!hasLoggedStructure)
        {
            LogStructure(result.handLandmarks[0]);
            hasLoggedStructure = true;
        }

        foreach (var landmarks in result.handLandmarks)
        {
            // Get the landmark list using reflection
            var landmarkList = GetLandmarkList(landmarks);

            if (landmarkList != null)
            {
                bool isStopSign = CheckForOpenPalm(landmarkList);

                if (isStopSign)
                {
                    Debug.Log("STOP SIGN DETECTED! Activating block...");
                    blockSystem.ActivateBlock();
                    return;
                }
            }
        }
    }

    private void LogStructure(Containers.NormalizedLandmarks landmarks)
    {
        var type = landmarks.GetType();
        Debug.Log($"=== NormalizedLandmarks Structure ===");
        Debug.Log($"Type: {type.FullName}");

        Debug.Log("Properties:");
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Debug.Log($"  {prop.Name} : {prop.PropertyType.Name}");
        }

        Debug.Log("Fields:");
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Debug.Log($"  {field.Name} : {field.FieldType.Name}");
        }
    }

    private bool CheckForOpenPalm(System.Collections.IList landmarkList)
    {
        try
        {
            if (landmarkList == null || landmarkList.Count < 21)
            {
                if (landmarkList == null)
                    Debug.LogError("Could not extract landmark list from NormalizedLandmarks");
                else
                    Debug.LogWarning($"Not enough landmarks: {landmarkList.Count}");
                return false;
            }

            // Check all four fingers are open AND extended
            bool indexOpen = IsFingerOpen(landmarkList, 8, 5);
            bool middleOpen = IsFingerOpen(landmarkList, 12, 9);
            bool ringOpen = IsFingerOpen(landmarkList, 16, 13);
            bool pinkyOpen = IsFingerOpen(landmarkList, 20, 17);

            // Also check thumb is extended (not tucked in)
            bool thumbExtended = IsFingerOpen(landmarkList, 4, 2);

            // For a true stop sign/open palm:
            // - ALL 4 fingers must be open
            // - Thumb should also be extended
            bool isOpenPalm = indexOpen && middleOpen && ringOpen && pinkyOpen && thumbExtended;

            // Debug individual finger states
            Debug.Log($"Finger states - Index:{indexOpen} Middle:{middleOpen} Ring:{ringOpen} Pinky:{pinkyOpen} Thumb:{thumbExtended}");

            if (isOpenPalm)
            {
                Debug.Log($"✋ Open palm detected! All fingers extended!");
            }

            return isOpenPalm;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CheckForOpenPalm: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    private System.Collections.IList GetLandmarkList(Containers.NormalizedLandmarks landmarks)
    {
        var type = landmarks.GetType();

        // Use cached property if type matches
        if (lastLandmarkType == type && landmarkListProperty != null)
        {
            var value = landmarkListProperty.GetValue(landmarks);
            if (value is System.Collections.IList list)
                return list;
        }

        // Try all possible property/field names
        string[] possibleNames = { "Landmark", "landmark", "Landmarks", "landmarks",
                                   "landmark_", "_landmark", "Landmark_", "_Landmark" };

        // Try properties first
        foreach (var name in possibleNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                var value = prop.GetValue(landmarks);
                if (value is System.Collections.IList list)
                {
                    // Cache the property for future calls
                    landmarkListProperty = prop;
                    lastLandmarkType = type;
                    Debug.Log($"Found landmark list via property '{name}', count: {list.Count}");
                    return list;
                }
            }
        }

        // Try fields
        foreach (var name in possibleNames)
        {
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var value = field.GetValue(landmarks);
                if (value is System.Collections.IList list)
                {
                    Debug.Log($"Found landmark list via field '{name}', count: {list.Count}");
                    return list;
                }
            }
        }

        return null;
    }

    private bool IsFingerOpen(System.Collections.IList landmarkList, int tipIndex, int mcpIndex)
    {
        try
        {
            if (landmarkList.Count <= tipIndex || landmarkList.Count <= mcpIndex)
                return false;

            var tipLandmark = landmarkList[tipIndex];
            var mcpLandmark = landmarkList[mcpIndex];

            float tipY = GetYCoordinate(tipLandmark);
            float mcpY = GetYCoordinate(mcpLandmark);

            bool isOpen = tipY < mcpY - FINGER_OPEN_THRESHOLD;

            return isOpen;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking finger {tipIndex}: {e.Message}");
            return false;
        }
    }

    private float GetYCoordinate(object landmark)
    {
        var type = landmark.GetType();

        // Use cached property if type matches
        if (lastLandmarkType == type && yCoordinateProperty != null)
        {
            try { return Convert.ToSingle(yCoordinateProperty.GetValue(landmark)); }
            catch { }
        }

        // Try all possible Y coordinate names
        string[] possibleNames = { "Y", "y", "Y_", "y_", "_y", "_Y" };

        // Try properties
        foreach (var name in possibleNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                try
                {
                    yCoordinateProperty = prop; // Cache it
                    lastLandmarkType = type;
                    return Convert.ToSingle(prop.GetValue(landmark));
                }
                catch { }
            }
        }

        // Try fields
        foreach (var name in possibleNames)
        {
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                try { return Convert.ToSingle(field.GetValue(landmark)); }
                catch { }
            }
        }

        Debug.LogError($"Could not find Y coordinate in type {type.Name}");
        return 0f;
    }

    void OnDestroy()
    {
        if (handRunner != null)
        {
            handRunner.OnHandResult -= OnHandLandmarkResult;
            Debug.Log("GestureFilter: Unsubscribed from HandLandmarker events.");
        }
    }
}