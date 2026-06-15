using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] private NetworkSceneManagerDefault sceneManager;
    [SerializeField] private NetworkPrefabRef chatManagerPrefab;

    public static UnityAction onJoinedLobby;
    public static UnityAction<bool> onNoSessionsActive;
    public static UnityAction<bool> onHostCheck;
    public static UnityAction<List<SessionInfo>> onSessionCreated;
    public static UnityAction<SessionInfo> onLocalPlayerJoined;
  
    private void OnEnable()
    {
        LobbyItemHandler.onLobbyJoined += JoinLobby;
        SessionListUiHandler.onSessionCreated += StartSession;
        SessionInfoListUiItem.onSessionJoin += StartSession;
        SessionListUiHandler.onHostStartedGame += StartMatch;
    }
    private void OnDisable()
    {
        LobbyItemHandler.onLobbyJoined -= JoinLobby;
        SessionListUiHandler.onSessionCreated -= StartSession;
        SessionInfoListUiItem.onSessionJoin -= StartSession;
        SessionListUiHandler.onHostStartedGame -= StartMatch;
    }

    private void Start()
    {
        networkRunner.AddCallbacks(this);
    }

    public async void JoinLobby(string lobbyName)
    {

        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);

        if (result.Ok)
        {
            onJoinedLobby.Invoke();
        }
        else
        {
            Debug.Log("couldn't connect the lobby");
        }
    }
    public async void StartSession(string sessionName)
    {
        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            PlayerCount = 10,
            OnGameStarted = OnGameStarted,
            CustomLobbyName = networkRunner.SessionInfo.Name,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start session: {result.ShutdownReason}");
        }
    }
    
    public async void StartMatch()
    {
        if (!networkRunner.IsSharedModeMasterClient)
            return;

        await networkRunner.LoadScene(SceneRef.FromIndex(1));
    }

    public void OnGameStarted(NetworkRunner obj)
    {
        bool isHost = networkRunner.IsSharedModeMasterClient;
        onHostCheck.Invoke(isHost);
        
        if (networkRunner.IsSharedModeMasterClient)
        {
            networkRunner.Spawn(chatManagerPrefab);
        }
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        onLocalPlayerJoined.Invoke(runner.SessionInfo);
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        onLocalPlayerJoined.Invoke(runner.SessionInfo);

    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
       
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (sessionList.Count <= 0)
        {
            onNoSessionsActive.Invoke(true);
        }
        else
        {
            onNoSessionsActive.Invoke(false);
        }

        onSessionCreated.Invoke(sessionList);
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {    }
}
