using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public partial class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] private NetworkSceneManagerDefault sceneManager;
    [SerializeField] private NetworkPrefabRef chatManagerPrefab;
    [SerializeField] private NetworkPrefabRef characterSelectionNetworkManager;
    [SerializeField] private bool autoJoinLobbyOnStart = true;
    [SerializeField] private string defaultLobbyName = "Default";
    [SerializeField] private int lobbySelectionSceneBuildIndex;
    [SerializeField] private int characterSelectionSceneBuildIndex = 1;
    private string _currentLobbyName;
    private bool _isJoiningLobby;
    private bool _isIntentionalShutdown;
    private bool _lastKnownHostState;
    private bool _hasKnownHostState;

    public NetworkRunner Runner => networkRunner;
    public string LocalPlayerNickname { get; private set; }

    public static event UnityAction JoinedLobby;
    public static event UnityAction<bool> NoSessionsStateChanged;
    public static event UnityAction<bool> HostStateChanged;
    public static event UnityAction<List<SessionInfo>> SessionListUpdated;
    public static event UnityAction<SessionInfo> SessionPlayerCountChanged;
    public static event UnityAction SessionStartSucceeded;
  
    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Duplicate NetworkManager detected.", this);
            enabled = false;
            return;
        }

        Instance = this;
        LobbyItemHandler.LobbyJoinRequested += JoinLobby;
        SessionListViewController.SessionCreateRequested += CreateSession;
        SessionListViewController.SessionSettingsChanged += UpdateSessionSettings;
        SessionListViewController.HostStartRequested += StartMatch;
        SessionListViewController.HostCloseRequested += CloseSessionForEveryone;
        SessionListViewController.LocalPlayerLeaveRequested += LeaveSession;
        SessionListViewController.BackToLobbyRequested += BackToLobbySelection;
    }
    private void OnDisable()
    {
        LobbyItemHandler.LobbyJoinRequested -= JoinLobby;
        SessionListViewController.SessionCreateRequested -= CreateSession;
        SessionListViewController.SessionSettingsChanged -= UpdateSessionSettings;
        SessionListViewController.HostStartRequested -= StartMatch;
        SessionListViewController.HostCloseRequested -= CloseSessionForEveryone;
        SessionListViewController.LocalPlayerLeaveRequested -= LeaveSession;
        SessionListViewController.BackToLobbyRequested -= BackToLobbySelection;

        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        networkRunner.AddCallbacks(this);

        if (autoJoinLobbyOnStart)
            JoinLobby(defaultLobbyName);
    }

    public void JoinLobby(string lobbyName)
    {
        AsyncTaskRunner.Run(
            JoinLobbyAsync(lobbyName),
            this,
            "Could not connect to the lobby."
        );
    }

    private async Task JoinLobbyAsync(string lobbyName)
    {
        if (string.IsNullOrWhiteSpace(lobbyName))
            lobbyName = defaultLobbyName;

        if (_isJoiningLobby)
            return;

        if (networkRunner.LobbyInfo.IsValid && networkRunner.LobbyInfo.Name == lobbyName)
        {
            _currentLobbyName = lobbyName;
            JoinedLobby?.Invoke();
            return;
        }

        _isJoiningLobby = true;
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
        _isJoiningLobby = false;

        if (result.Ok)
        {
            _currentLobbyName = lobbyName;
            JoinedLobby?.Invoke();
        }
        else
        {
            ReportServerError($"Couldn't connect to lobby: {GetStartGameError(result)}");
        }
    }
    public void CreateSession(SessionCreateRequest request)
    {
        SetLocalPlayerNickname(request.PlayerNickname);
        AsyncTaskRunner.Run(
            StartSessionAsync(request.SessionName, request.ShowInLobby, request.GameMode, request.Map, request.Region, request.PlayerCount, request.PasswordProtected, request.Password),
            this,
            "Could not create the session."
        );
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

    public void StartSession(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(LocalPlayerNickname))
        {
            ReportServerError("Enter your nickname.");
            return;
        }

        AsyncTaskRunner.Run(
            StartSessionAsync(sessionName, true, "Default", "Default", "Default", 4, false, string.Empty),
            this,
            "Could not join the session."
        );
    }

    private async Task StartSessionAsync(string sessionName, bool isVisible, string gameMode, string map, string region, int playerCount, bool passwordProtected, string password)
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

        SessionStartSucceeded?.Invoke();
    }
    
    public void StartMatch()
    {
        AsyncTaskRunner.Run(
            StartMatchAsync(),
            this,
            "Could not start the match."
        );
    }

    private async Task StartMatchAsync()
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

        if (!IsValidSceneBuildIndex(characterSelectionSceneBuildIndex))
        {
            ReportServerError(
                $"Invalid character-selection scene build index: {characterSelectionSceneBuildIndex}"
            );
            return;
        }

        await networkRunner.LoadScene(
            SceneRef.FromIndex(characterSelectionSceneBuildIndex)
        );
    }

    public void CloseSessionForEveryone()
    {
        if (!networkRunner.IsRunning || !networkRunner.SessionInfo.IsValid)
        {
            LeaveSession();
            return;
        }

        if (!networkRunner.IsSharedModeMasterClient)
        {
            LeaveSession();
            return;
        }

        networkRunner.SessionInfo.IsOpen = false;
        networkRunner.SessionInfo.IsVisible = false;

        CharacterSelectionNetworkManager sessionManager = CharacterSelectionNetworkManager.Instance;

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

    public void LeaveSession()
    {
        AsyncTaskRunner.Run(
            ShutdownAndLoadLobbySelection(),
            this,
            "Could not leave the session."
        );
    }

    public void LeaveSessionAfterDelay(float delay)
    {
        StartCoroutine(LeaveSessionAfterDelayRoutine(delay));
    }

    public void BackToLobbySelection()
    {
        LeaveSession();
    }

    public async System.Threading.Tasks.Task ShutdownAndLoadLobbySelection()
    {
        _isIntentionalShutdown = true;

        if (networkRunner != null && networkRunner.IsRunning)
            await networkRunner.Shutdown();

        if (!IsValidSceneBuildIndex(lobbySelectionSceneBuildIndex))
        {
            ReportServerError(
                $"Invalid lobby scene build index: {lobbySelectionSceneBuildIndex}"
            );
            return;
        }

        SceneManager.LoadScene(lobbySelectionSceneBuildIndex);
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
        ErrorMessagePresenter.ReportError(message);
    }

    private static string GetStartGameError(StartGameResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            return result.ErrorMessage;

        return result.ShutdownReason.ToString();
    }

    private static bool IsValidSceneBuildIndex(int buildIndex)
    {
        return buildIndex >= 0 &&
               buildIndex < SceneManager.sceneCountInBuildSettings;
    }

    private void RefreshHostState(bool forceNotify, string reason)
    {
        if (networkRunner == null || !networkRunner.IsRunning)
            return;

        bool isHost = networkRunner.IsSharedModeMasterClient;
        bool changed = !_hasKnownHostState || _lastKnownHostState != isHost;

        _hasKnownHostState = true;
        _lastKnownHostState = isHost;

        if (changed || forceNotify)
            HostStateChanged?.Invoke(isHost);

        if (!isHost)
            return;

        CharacterSelectionNetworkManager.Instance?.RefreshMasterClientState();
        ReadyManager.Instance?.EvaluateAllPlayersReady();

        Debug.Log($"Host state refreshed: {reason}");
    }
}
