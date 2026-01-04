using UnityEngine;
using System.Collections;
using TMPro; // Needed for TextMeshPro text

public class PopupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject snipePopupPanel; // Assign the SnipePopup Panel here!
    public TextMeshProUGUI successText; // Assign the SuccessText here!

    private Coroutine popupCoroutine;
    private const float DISPLAY_DURATION = 2.0f; // Display for 2 seconds

    // This function is called by the SniperSystem script when a snipe succeeds
    public void ShowSuccessPopup()
    {
        // Stop any existing popup timer to prevent overlap
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        // Start the timed display sequence
        popupCoroutine = StartCoroutine(PopupSequence());
    }

    private IEnumerator PopupSequence()
    {
        // 1. Show the panel
        snipePopupPanel.SetActive(true);

        // 2. Wait for 2 seconds (the display duration)
        yield return new WaitForSeconds(DISPLAY_DURATION);

        // 3. Hide the panel
        snipePopupPanel.SetActive(false);
        popupCoroutine = null;
    }
}