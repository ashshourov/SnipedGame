// using UnityEngine;
// using UnityEngine.UI;
// using Fusion;
// using Fusion.Sockets;
// using System.Collections.Generic;
// using System;
// using UnityEngine.SceneManagement;

// public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
// {
//     [Header("UI References")]
//     public InputField partyCodeInputField;
//     public Button startButton;
//     public Text statusText;

//     private NetworkRunner _runner;

//     // Call this from your UI Button (e.g., "Join/Create Room")
//     public async void JoinOrCreateRoom()
//     {
//         string roomCode = partyCodeInputField.text.ToUpper();
        
//         if (string.IsNullOrEmpty(roomCode) || roomCode.Length < 4)
//         {
//             SetStatus("Invalid Code (Min 4 chars)");
//             return;
//         }

//         if (_runner == null)
//         {
//             _runner = gameObject.AddComponent<NetworkRunner>();
//             _runner.ProvideInput = true;
//         }

//         SetStatus("Connecting to " + roomCode + "...");

//         // Start the game in Shared Mode (Ideal for Mobile/Casual)
//         var result = await _runner.StartGame(new StartGameArgs()
//         {
//             GameMode = GameMode.Shared,
//             SessionName = roomCode, // Use the Party Code as the Session Name
//             Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex + 1), // Load next scene
//             SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
//         });

//         if (result.Ok)
//         {
//             SetStatus("Connected!");
//         }
//         else
//         {
//             SetStatus("Error: " + result.ShutdownReason);
//         }
//     }

//     private void SetStatus(string msg)
//     {
//         if (statusText != null) statusText.text = msg;
//         Debug.Log($"[Lobby]: {msg}");
//     }

//     // --- INetworkRunnerCallbacks (Required Implementations) ---
//     public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
//     public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
//     public void OnInput(NetworkRunner runner, NetworkInput input) { }
//     public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
//     public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
//     public void OnConnectedToServer(NetworkRunner runner) { }
//     public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
//     public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
//     public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
//     public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
//     public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
//     public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
//     public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
//     public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
//     public void OnSceneLoadDone(NetworkRunner runner) { }
//     public void OnSceneLoadStart(NetworkRunner runner) { }
// }