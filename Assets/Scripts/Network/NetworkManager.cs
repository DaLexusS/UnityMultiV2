using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] private NetworkSceneManagerDefault sceneManager;
    [SerializeField] private NetworkPrefabRef chatManagerPrefab;
    [SerializeField] private NetworkPrefabRef characterSelectionNetworkManager;
    private string _currentLobbyName;

    public NetworkRunner Runner => networkRunner;

    public static UnityAction onJoinedLobby;
    public static UnityAction<bool> onNoSessionsActive;
    public static UnityAction<bool> onHostCheck;
    public static UnityAction<List<SessionInfo>> onSessionCreated;
    public static UnityAction<SessionInfo> onLocalPlayerJoined;
  
    private void OnEnable()
    {
        Instance = this;
        LobbyItemHandler.onLobbyJoined += JoinLobby;
        SessionListUiHandler.onSessionCreated += CreateSession;
        SessionListUiHandler.onSessionSettingsChanged += UpdateSessionSettings;
        SessionInfoListUiItem.onSessionJoin += StartSession;
        SessionListUiHandler.onHostStartedGame += StartMatch;
        SessionListUiHandler.onHostClosedSession += CloseSessionForEveryone;
        SessionListUiHandler.onLocalPlayerLeftSession += LeaveSession;
        SessionListUiHandler.onBackToLobbySelection += BackToLobbySelection;
    }
    private void OnDisable()
    {
        LobbyItemHandler.onLobbyJoined -= JoinLobby;
        SessionListUiHandler.onSessionCreated -= CreateSession;
        SessionListUiHandler.onSessionSettingsChanged -= UpdateSessionSettings;
        SessionInfoListUiItem.onSessionJoin -= StartSession;
        SessionListUiHandler.onHostStartedGame -= StartMatch;
        SessionListUiHandler.onHostClosedSession -= CloseSessionForEveryone;
        SessionListUiHandler.onLocalPlayerLeftSession -= LeaveSession;
        SessionListUiHandler.onBackToLobbySelection -= BackToLobbySelection;

        if (Instance == this)
            Instance = null;
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
            _currentLobbyName = lobbyName;
            onJoinedLobby.Invoke();
        }
        else
        {
            Debug.Log("couldn't connect the lobby");
        }
    }
    public async void CreateSession(SessionCreateRequest request)
    {
        await StartSession(request.SessionName, request.ShowInLobby, request.GameMode, request.Map);
    }

    public void UpdateSessionSettings(SessionCreateRequest request)
    {
        if (!networkRunner.IsRunning || !networkRunner.SessionInfo.IsValid || !networkRunner.IsSharedModeMasterClient)
            return;

        networkRunner.SessionInfo.IsVisible = request.ShowInLobby;
        networkRunner.SessionInfo.UpdateCustomProperties(SessionMetadata.CreateProperties(
            request.GameMode,
            request.Map,
            GetCurrentSessionProperty(SessionMetadata.StateKey, SessionMetadata.StateLobby)));

        Debug.Log($"Updated session settings: {SessionMetadata.GetDebugDescription(networkRunner.SessionInfo)}");
    }

    public async void StartSession(string sessionName)
    {
        await StartSession(sessionName, true, "Default", "Default");
    }

    private async System.Threading.Tasks.Task StartSession(string sessionName, bool isVisible, string gameMode, string map)
    {
        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            PlayerCount = 10,
            OnGameStarted = OnGameStarted,
            CustomLobbyName = GetCurrentLobbyName(),
            SceneManager = sceneManager,
            IsVisible = isVisible,
            IsOpen = true,
            SessionProperties = SessionMetadata.CreateProperties(gameMode, map, SessionMetadata.StateLobby)
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

        networkRunner.SessionInfo.IsOpen = false;
        networkRunner.SessionInfo.UpdateCustomProperties(SessionMetadata.CreateProperties(
            GetCurrentSessionProperty(SessionMetadata.GameModeKey, "Default"),
            GetCurrentSessionProperty(SessionMetadata.MapKey, "Default"),
            SessionMetadata.StateStarted));

        await networkRunner.LoadScene(SceneRef.FromIndex(1));
    }

    public void CloseSessionForEveryone()
    {
        if (!networkRunner.IsRunning || !networkRunner.SessionInfo.IsValid || !networkRunner.IsSharedModeMasterClient)
            return;

        networkRunner.SessionInfo.IsOpen = false;
        networkRunner.SessionInfo.IsVisible = false;

        CharacterSelectionNetworkManager sessionManager = FindAnyObjectByType<CharacterSelectionNetworkManager>();

        if (sessionManager != null)
        {
            sessionManager.RPC_CloseSessionForEveryone();
        }
        else
        {
            Debug.LogWarning("Could not find CharacterSelectionNetworkManager. Closing only the local session.");
            LeaveSession();
        }
    }

    public async void LeaveSession()
    {
        await ShutdownAndLoadLobbySelection();
    }

    public void LeaveSessionAfterDelay(float delay)
    {
        StartCoroutine(LeaveSessionAfterDelayRoutine(delay));
    }

    public async void BackToLobbySelection()
    {
        await ShutdownAndLoadLobbySelection();
    }

    public async System.Threading.Tasks.Task ShutdownAndLoadLobbySelection()
    {
        if (networkRunner != null && networkRunner.IsRunning)
            await networkRunner.Shutdown();

        SceneManager.LoadScene(0);
    }

    private IEnumerator LeaveSessionAfterDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        LeaveSession();
    }

    private string GetCurrentSessionProperty(string key, string fallback)
    {
        return SessionMetadata.TryGetValue(networkRunner.SessionInfo, key, out string value) ? value : fallback;
    }

    private string GetCurrentLobbyName()
    {
        if (networkRunner.LobbyInfo.IsValid)
            return networkRunner.LobbyInfo.Name;

        return _currentLobbyName;
    }

    public void OnGameStarted(NetworkRunner obj)
    {
        bool isHost = networkRunner.IsSharedModeMasterClient;
        onHostCheck.Invoke(isHost);
        
        if (networkRunner.IsSharedModeMasterClient)
        {
            NetworkObject chatMan = networkRunner.Spawn(chatManagerPrefab);
            DontDestroyOnLoad(chatMan);
            NetworkObject charSelMan =networkRunner.Spawn(characterSelectionNetworkManager);
            DontDestroyOnLoad(charSelMan);
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
        SessionMetadata.LogSessionList("NetworkManager.OnSessionListUpdated", sessionList);

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
