using UnityEngine;
using UnityEngine.Android;

public class CameraPermissionManager : MonoBehaviour
{
    void Start()
    {
        // Request camera permission on Android
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        // Wait a frame before starting camera
        StartCoroutine(InitializeCameraAfterPermission());
    }

    System.Collections.IEnumerator InitializeCameraAfterPermission()
    {
        // Wait for permission dialog
        yield return new WaitForSeconds(0.5f);

        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("Camera permission granted!");
            // Your camera initialization code here
        }
        else
        {
            Debug.LogError("Camera permission denied!");
        }
    }
}