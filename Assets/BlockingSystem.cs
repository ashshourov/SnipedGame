using UnityEngine;
using System.Collections;
using TMPro; // Assuming you have a UI element for the timer

public class BlockSystem : MonoBehaviour
{
    // CRITICAL: This static flag is checked by the SniperSystem.
    public static bool isBlocking = false;

    [Header("Block Settings")]
    [Tooltip("The duration the player is immune after a successful gesture.")]
    private const float BLOCK_DURATION = 30.0f; // As per proposal
    private float blockTimer = 0f;

    [Header("UI Reference")]
    public TextMeshProUGUI blockTimerText; // Display timer on screen

    private bool timerRunning = false;

    void Update()
    {
        // Decrement timer only when the block is active
        if (timerRunning)
        {
            blockTimer -= Time.deltaTime;
            UpdateUITimer();

            if (blockTimer <= 0)
            {
                DeactivateBlock();
            }
        }
    }

    // This function must be linked to the MediaPipe Hand Graph's output event.
    public void ActivateBlock()
    {
        if (isBlocking)
        {
            Debug.Log("Block already active. Refreshing timer.");
            // Optional: You could allow the player to refresh the timer here
        }
        else
        {
            isBlocking = true;
            timerRunning = true;
            blockTimer = BLOCK_DURATION;
            Debug.Log("DEFENSE ACTIVATED: Block mode engaged!");
        }
    }

    void DeactivateBlock()
    {
        isBlocking = false;
        timerRunning = false;
        blockTimer = 0f;

        if (blockTimerText != null)
        {
            blockTimerText.text = "";
        }
        Debug.Log("DEFENSE EXPIRED: Block mode ended.");
    }

    void UpdateUITimer()
    {
        if (blockTimerText != null)
        {
            blockTimerText.text = $"Block: {Mathf.CeilToInt(blockTimer)}s";
            blockTimerText.color = isBlocking ? Color.cyan : Color.white;
        }
    }

    // Safety check to ensure UI is hidden at start
    void Start()
    {
        DeactivateBlock();
    }
}