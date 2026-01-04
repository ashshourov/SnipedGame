using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System.Collections.Generic;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }
    // Inside PlayFabManager.cs, near the top:
    public bool IsAuthenticated { get; private set; } = false;

    [Header("PlayFab Settings")]
    public string titleId = "YOUR_TITLE_ID"; // Enter your PlayFab Title ID

    [Header("Player Info")]
    private string playerId;
    private string playerDisplayName;
    private bool isLoggedIn = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Set the Title ID
            if (!string.IsNullOrEmpty(titleId))
            {
                PlayFabSettings.staticSettings.TitleId = titleId;
                Debug.Log($"PlayFab initialized with Title ID: {titleId}");
            }
            else
            {
                Debug.LogError("PlayFab Title ID is not set!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==================== AUTHENTICATION ====================

    /// <summary>
    /// Register a new user with email and password
    /// </summary>
    public Task<bool> RegisterUser(string email, string password, string username)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            Username = username,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            result =>
            {
                Debug.Log($"Registration successful! PlayFab ID: {result.PlayFabId}");
                playerId = result.PlayFabId;
                playerDisplayName = username;
                isLoggedIn = true;
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"Registration failed: {error.ErrorMessage}");
                tcs.SetResult(false);
            });

        return tcs.Task;
    }

    /// <summary>
    /// Sign in with email and password
    /// </summary>
    public Task<bool> SignInUser(string email, string password)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            result =>
            {
                Debug.Log($"Login successful! PlayFab ID: {result.PlayFabId}");
                playerId = result.PlayFabId;
                isLoggedIn = true;

                // Get player profile to retrieve username
                GetPlayerProfile();

                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"Login failed: {error.ErrorMessage}");
                tcs.SetResult(false);
            });

        return tcs.Task;
    }

    /// <summary>
    /// Sign in as guest (anonymous)
    /// </summary>
    public Task<bool> SignInGuest()
    {
        var tcs = new TaskCompletionSource<bool>();

        // Use Android Device ID for guest login (more reliable)
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
        {
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            TitleId = titleId
        };

        PlayFabClientAPI.LoginWithAndroidDeviceID(request,
            result =>
            {
                Debug.Log($"Guest login successful! PlayFab ID: {result.PlayFabId}");
                playerId = result.PlayFabId;
                playerDisplayName = "Guest";
                isLoggedIn = true;
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"Guest login failed: {error.ErrorMessage}");
                tcs.SetResult(false);
            });
#else
        // For iOS or other platforms, use CustomID
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = deviceId,
            CreateAccount = true,
            TitleId = titleId
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                Debug.Log($"Guest login successful! PlayFab ID: {result.PlayFabId}");
                playerId = result.PlayFabId;
                playerDisplayName = "Guest";
                isLoggedIn = true;
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"Guest login failed: {error.ErrorMessage}");
                tcs.SetResult(false);
            });
#endif

        return tcs.Task;
    }

    /// <summary>
    /// Get player profile information
    /// </summary>
    private void GetPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            PlayFabId = playerId
        };

        PlayFabClientAPI.GetPlayerProfile(request,
            result =>
            {
                if (result.PlayerProfile != null)
                {
                    playerDisplayName = result.PlayerProfile.DisplayName ?? "Player";
                    Debug.Log($"Player display name: {playerDisplayName}");
                }
            },
            error =>
            {
                Debug.LogWarning($"Failed to get player profile: {error.ErrorMessage}");
            });
    }

    // ==================== PLAYER DATA ====================

    /// <summary>
    /// Save player data to PlayFab
    /// </summary>
    public Task<bool> SavePlayerData(string key, string value)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { key, value }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log($"Player data saved: {key}");
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"Failed to save player data: {error.ErrorMessage}");
                tcs.SetResult(false);
            });

        return tcs.Task;
    }

    /// <summary>
    /// Load player data from PlayFab
    /// </summary>
    public Task<string> LoadPlayerData(string key)
    {
        var tcs = new TaskCompletionSource<string>();

        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request,
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey(key))
                {
                    string value = result.Data[key].Value;
                    Debug.Log($"Player data loaded: {key} = {value}");
                    tcs.SetResult(value);
                }
                else
                {
                    Debug.LogWarning($"Key not found: {key}");
                    tcs.SetResult(null);
                }
            },
            error =>
            {
                Debug.LogError($"Failed to load player data: {error.ErrorMessage}");
                tcs.SetResult(null);
            });

        return tcs.Task;
    }

    // ==================== TEAM/GAME DATA ====================

    /// <summary>
    /// Send snipe confirmation to server
    /// </summary>
    public Task<bool> SendSnipeConfirmation(string targetPlayerId)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Create snipe data object
        SnipeData snipeData = new SnipeData
        {
            sniper_id = playerId,
            target_id = targetPlayerId,
            timestamp = System.DateTime.UtcNow.ToString()
        };

        // Convert to JSON using Unity's JsonUtility
        string jsonData = JsonUtility.ToJson(snipeData);

        // Save to player's internal data (you can also use Cloud Script)
        SavePlayerData($"snipe_{System.DateTime.UtcNow.Ticks}", jsonData);

        Debug.Log($"Snipe confirmed: {playerId} → {targetPlayerId}");
        tcs.SetResult(true);

        return tcs.Task;
    }

    /// <summary>
    /// Get team members list
    /// </summary>
    public Task<List<PlayerInfo>> GetTeamMembers()
    {
        var tcs = new TaskCompletionSource<List<PlayerInfo>>();

        // TODO: Implement based on your team system
        // For now, return dummy data
        var dummyTeam = new List<PlayerInfo>
        {
            new PlayerInfo { PlayerId = "player1", DisplayName = "Player 1" },
            new PlayerInfo { PlayerId = "player2", DisplayName = "Player 2" },
            new PlayerInfo { PlayerId = "player3", DisplayName = "Player 3" }
        };

        tcs.SetResult(dummyTeam);
        return tcs.Task;
    }

    // ==================== UTILITY ====================

    public bool IsLoggedIn() => isLoggedIn;
    public string GetPlayerId() => playerId;
    public string GetPlayerName() => playerDisplayName;

    public void SignOut()
    {
        playerId = null;
        playerDisplayName = null;
        isLoggedIn = false;
        Debug.Log("Player signed out");
    }
}

// Helper class for player info
[System.Serializable]
public class PlayerInfo
{
    public string PlayerId;
    public string DisplayName;
}

// Helper class for snipe data
[System.Serializable]
public class SnipeData
{
    public string sniper_id;
    public string target_id;
    public string timestamp;
}