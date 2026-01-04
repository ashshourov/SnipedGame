using UnityEngine;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI & Logic")]
    public GameObject arGameCanvas; // Assign your Main Canvas here
    public GameObject[] gameLogicComponents; // Array of GameObjects containing the core logic (Runners, etc.)

    void Awake()
    {
        // 1. Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // We only proceed if we are in the main game scene.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainFile")
        {
            // Start the initialization sequence
            InitializeGameLogic();
        }
    }

    private async void InitializeGameLogic()
    {
        // 2. Wait for Authentication
        // This is necessary because the Login Scene loads the MainFile scene immediately,
        // but the PlayFab/Firebase sign-in might still be running.

        Debug.Log("GameManager: Waiting for PlayFab authentication...");

        // FIX: Wait until the PlayFab Manager confirms the user is signed in.
        // NOTE: If using PlayFabManager, ensure the IsAuthenticated property is public.
        while (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("GameManager: Authentication not complete. Waiting...");
            await Task.Delay(500); // Wait 0.5 seconds
        }

        Debug.Log("GameManager: User authenticated! Activating game logic.");

        // 3. Activate Game Logic Components
        SetLogicActive(true);

        // 4. Start AR Camera (The MediaPipe runner's Start() will execute here)
        // Since the components are now active, their own Start() and Awake() methods will run,
        // initiating the camera and detection graphs.
    }

    /// <summary>
    /// Enables or disables the core detection and game logic objects.
    /// </summary>
    /// <param name="active">True to enable logic, false to disable.</param>
    public void SetLogicActive(bool active)
    {
        // Enable or disable the main Canvas containing the Target Selection UI, etc.
        if (arGameCanvas != null)
        {
            arGameCanvas.SetActive(active);
        }

        // Enable or disable the runners and detection scripts (e.g., Hand/Face Graphs)
        foreach (GameObject obj in gameLogicComponents)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }

        if (!active)
        {
            // Optional: Pause the WebCamTexture here for optimization when paused.
        }
    }
}