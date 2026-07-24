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
    [SerializeField] private bool autoJoinLobbyOnStart = true;
    [SerializeField] private string defaultLobbyName = "Default";
    private string _currentLobbyName;
    private bool _isJoiningLobby;
    private bool _isIntentionalShutdown;
    private bool _lastKnownHostState;
    private bool _hasKnownHostState;

    public NetworkRunner Runner => networkRunner;
    public string LocalPlayerNickname { get; private set; }

    public static UnityAction onJoinedLobby;
    public static UnityAction<bool> onNoSessionsActive;
    public static UnityAction<bool> onHostCheck;
    public static UnityAction<List<SessionInfo>> onSessionCreated;
    public static UnityAction<SessionInfo> onLocalPlayerJoined;
    public static UnityAction onSessionStartSucceeded;
  
    private void OnEnable()
    {
        Instance = this;
        LobbyItemHandler.onLobbyJoined += JoinLobby;
        SessionListUiHandler.onSessionCreated += CreateSession;
        SessionListUiHandler.onSessionSettingsChanged += UpdateSessionSettings;
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

        if (autoJoinLobbyOnStart)
            JoinLobby(defaultLobbyName);
    }

    public async void JoinLobby(string lobbyName)
    {
        if (string.IsNullOrWhiteSpace(lobbyName))
            lobbyName = defaultLobbyName;

        if (_isJoiningLobby)
            return;

        if (networkRunner.LobbyInfo.IsValid && networkRunner.LobbyInfo.Name == lobbyName)
        {
            _currentLobbyName = lobbyName;
            onJoinedLobby?.Invoke();
            return;
        }

        _isJoiningLobby = true;
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
        _isJoiningLobby = false;

        if (result.Ok)
        {
            _currentLobbyName = lobbyName;
            onJoinedLobby?.Invoke();
        }
        else
        {
            ReportServerError($"Couldn't connect to lobby: {GetStartGameError(result)}");
        }
    }
    public async void CreateSession(SessionCreateRequest request)
    {
        SetLocalPlayerNickname(request.PlayerNickname);
        await StartSession(request.SessionName, request.ShowInLobby, request.GameMode, request.Map, request.Region, request.PlayerCount, request.PasswordProtected, request.Password);
    }

    public void UpdateSessionSettings(SessionCreateRequest request)
    {
        if (!networkRunner.IsRunning || !networkRunner.SessionInfo.IsValid || !networkRunner.IsSharedModeMasterClient)
            return;

        networkRunner.SessionInfo.IsVisible = request.ShowInLobby;
        networkRunner.SessionInfo.UpdateCustomProperties(SessionMetadata.CreateProperties(
            request.GameMode,
            request.Map,
            request.Region,
            GetCurrentSessionProperty(SessionMetadata.StateKey, SessionMetadata.StateLobby),
            IsCurrentSessionPasswordProtected(),
            GetCurrentSessionProperty(SessionMetadata.PasswordKey, string.Empty)));

        Debug.Log($"Updated session settings: {SessionMetadata.GetDebugDescription(networkRunner.SessionInfo)}");
    }

    public void SetLocalPlayerNickname(string nickname)
    {
        LocalPlayerNickname = string.IsNullOrWhiteSpace(nickname) ? string.Empty : nickname.Trim();
    }

    public void SubmitLocalNickname()
    {
        if (string.IsNullOrWhiteSpace(LocalPlayerNickname))
            return;

        CharacterSelectionNetworkManager sessionManager = CharacterSelectionNetworkManager.Instance;

        if (sessionManager == null)
            return;

        sessionManager.RPC_RegisterNickname(LocalPlayerNickname);
    }

    public void KickPlayer(PlayerRef player)
    {
        CharacterSelectionNetworkManager sessionManager = CharacterSelectionNetworkManager.Instance;

        if (sessionManager == null)
        {
            ReportServerError("Could not kick player. Lobby player manager is not ready.");
            return;
        }

        sessionManager.KickPlayer(player);
    }

    public async void StartSession(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(LocalPlayerNickname))
        {
            ReportServerError("Enter your nickname.");
            return;
        }

        await StartSession(sessionName, true, "Default", "Default", "Default", 4, false, string.Empty);
    }

    private async System.Threading.Tasks.Task StartSession(string sessionName, bool isVisible, string gameMode, string map, string region, int playerCount, bool passwordProtected, string password)
    {
        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            PlayerCount = Mathf.Clamp(playerCount, 3, 4),
            OnGameStarted = OnGameStarted,
            CustomLobbyName = GetCurrentLobbyName(),
            SceneManager = sceneManager,
            IsVisible = isVisible,
            IsOpen = true,
            SessionProperties = SessionMetadata.CreateProperties(gameMode, map, region, SessionMetadata.StateLobby, passwordProtected, password)
        });

        if (!result.Ok)
        {
            ReportServerError($"Failed to start session: {GetStartGameError(result)}");
            return;
        }

        onSessionStartSucceeded?.Invoke();
    }
    
    public async void StartMatch()
    {
        if (!networkRunner.IsSharedModeMasterClient)
        {
            ReportServerError("Only the host can start the match.");
            return;
        }

        networkRunner.SessionInfo.IsOpen = false;
        networkRunner.SessionInfo.UpdateCustomProperties(SessionMetadata.CreateProperties(
            GetCurrentSessionProperty(SessionMetadata.GameModeKey, "Default"),
            GetCurrentSessionProperty(SessionMetadata.MapKey, "Default"),
            GetCurrentSessionProperty(SessionMetadata.RegionKey, "Default"),
            SessionMetadata.StateStarted,
            IsCurrentSessionPasswordProtected(),
            GetCurrentSessionProperty(SessionMetadata.PasswordKey, string.Empty)));

        BroadcastMatchSettingsJson();

        await networkRunner.LoadScene(SceneRef.FromIndex(1));
    }

    public void CloseSessionForEveryone()
    {
        if (!networkRunner.IsRunning || !networkRunner.SessionInfo.IsValid || !networkRunner.IsSharedModeMasterClient)
        {
            ReportServerError("Only the host can close this session.");
            return;
        }

        networkRunner.SessionInfo.IsOpen = false;
        networkRunner.SessionInfo.IsVisible = false;

        CharacterSelectionNetworkManager sessionManager = FindAnyObjectByType<CharacterSelectionNetworkManager>();

        if (sessionManager != null)
        {
            sessionManager.RPC_CloseSessionForEveryone();
        }
        else
        {
            ReportServerError("Could not close the session for everyone. Closing your local session only.");
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
        _isIntentionalShutdown = true;

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

    private void BroadcastMatchSettingsJson()
    {
        MatchSettingsSync settingsSync = MatchSettingsSync.Instance;

        if (settingsSync == null)
        {
            ReportServerError("Could not send match settings. Settings manager is not ready.");
            return;
        }

        settingsSync.BroadcastSettings(new MatchSettingsJson
        {
            GameMode = GetCurrentSessionProperty(SessionMetadata.GameModeKey, "Default"),
            Map = GetCurrentSessionProperty(SessionMetadata.MapKey, "Default"),
            Region = GetCurrentSessionProperty(SessionMetadata.RegionKey, "Default"),
            MaxPlayers = networkRunner.SessionInfo.MaxPlayers,
            PasswordProtected = IsCurrentSessionPasswordProtected()
        });
    }

    private bool IsCurrentSessionPasswordProtected()
    {
        return GetCurrentSessionProperty(SessionMetadata.PasswordProtectedKey, "false") == "true";
    }

    private string GetCurrentLobbyName()
    {
        if (networkRunner.LobbyInfo.IsValid)
            return networkRunner.LobbyInfo.Name;

        if (!string.IsNullOrWhiteSpace(_currentLobbyName))
            return _currentLobbyName;

        return defaultLobbyName;
    }

    private void ReportServerError(string message)
    {
        ErrorHandlerUi.ReportError(message);
    }

    private static string GetStartGameError(StartGameResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            return result.ErrorMessage;

        return result.ShutdownReason.ToString();
    }

    public void OnGameStarted(NetworkRunner obj)
    {
        RefreshHostState(true, "Game started");
        
        if (networkRunner.IsSharedModeMasterClient)
        {
            NetworkObject chatMan = networkRunner.Spawn(chatManagerPrefab);
            DontDestroyOnLoad(chatMan);
            NetworkObject charSelMan =networkRunner.Spawn(characterSelectionNetworkManager);
            DontDestroyOnLoad(charSelMan);
        }

        SubmitLocalNickname();
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        onLocalPlayerJoined?.Invoke(runner.SessionInfo);

        if (player == runner.LocalPlayer)
            SubmitLocalNickname();

        RefreshHostState(false, "Player joined");
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsSharedModeMasterClient && CharacterSelectionNetworkManager.Instance != null)
            CharacterSelectionNetworkManager.Instance.RemovePlayer(player);

        onLocalPlayerJoined?.Invoke(runner.SessionInfo);
        RefreshHostState(true, "Player left");
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (_isIntentionalShutdown)
            return;

        if (shutdownReason != ShutdownReason.Ok)
            ReportServerError($"Network shutdown: {shutdownReason}");
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        if (_isIntentionalShutdown)
            return;

        ReportServerError($"Disconnected from server: {reason}");
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        ReportServerError($"Connection failed: {reason}");
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
            onNoSessionsActive?.Invoke(true);
        }
        else
        {
            onNoSessionsActive?.Invoke(false);
        }

        onSessionCreated?.Invoke(sessionList);
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        RefreshHostState(true, "Host migration");
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        RefreshHostState(true, "Scene load done");
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {    }

    private void RefreshHostState(bool forceNotify, string reason)
    {
        if (networkRunner == null || !networkRunner.IsRunning)
            return;

        bool isHost = networkRunner.IsSharedModeMasterClient;
        bool changed = !_hasKnownHostState || _lastKnownHostState != isHost;

        _hasKnownHostState = true;
        _lastKnownHostState = isHost;

        if (changed || forceNotify)
            onHostCheck?.Invoke(isHost);

        if (!isHost)
            return;

        CharacterSelectionNetworkManager.Instance?.RefreshMasterClientState();
        ReadyManager.Instance?.EvaluateAllPlayersReady();

        Debug.Log($"Host state refreshed: {reason}");
    }
}
