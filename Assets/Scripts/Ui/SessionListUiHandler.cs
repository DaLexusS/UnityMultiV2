using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class SessionListUiHandler : MonoBehaviour
{
    public static UnityAction<SessionCreateRequest> onSessionCreated;
    public static UnityAction<SessionCreateRequest> onSessionSettingsChanged;
    public static UnityAction onHostStartedGame;
    public static UnityAction onHostClosedSession;
    public static UnityAction onLocalPlayerLeftSession;
    public static UnityAction onBackToLobbySelection;

    public TextMeshProUGUI statusText;
    
    public GameObject sessionItemListPrefab;
    public GameObject sessionList;
    public GameObject LobbyList;
    public GameObject playerCounterUi;
    [SerializeField] private GameObject sessionCreateLabel;
    [SerializeField] private GameObject joinLobbyPanel;
    public GameObject StartGameButton;
    [SerializeField] private GameObject closeSessionButton;
    [SerializeField] private GameObject leaveSessionButton;
    [SerializeField] private PlayerDataUi playerDataPrefab;
    [SerializeField] private Transform playersListParent;
    public TMP_Text playerCountInLobby;
    [Header("Session Filters")]
    [SerializeField] private Toggle showInLobbyToggle;
    [SerializeField] private TMP_Dropdown createGameModeDropdown;
    [SerializeField] private TMP_Dropdown createMapDropdown;
    [SerializeField] private TMP_Dropdown createRegionDropdown;
    [SerializeField] private Toggle passwordToggle;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private GameObject passwordFieldObject;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_InputField nicknameInputField;
    [Header("Join Lobby")]
    [SerializeField] private TMP_InputField joinNicknameInputField;
    [SerializeField] private TMP_InputField joinPasswordInputField;
    [SerializeField] private GameObject joinPasswordFieldObject;
    [SerializeField] private GameObject joinPasswordTitleObject;
    [SerializeField] private int minPlayerCount = 2;
    [SerializeField] private int maxPlayerCount = 4;
    [SerializeField] private TMP_Dropdown searchGameModeDropdown;
    [SerializeField] private TMP_Dropdown preferredMapDropdown;
    [SerializeField] private TMP_Dropdown searchRegionDropdown;

    private string _lastName;
    private string _lastNickname;
    private SessionInfo _selectedJoinSession;
    private readonly List<SessionInfo> _latestSessions = new List<SessionInfo>();
    private readonly List<PlayerDataUi> _playerDataRows = new List<PlayerDataUi>();

    public VerticalLayoutGroup verticalLayoutGroup;

    private string _lastLobbyName;

    private void OnEnable()
    {
        NetworkManager.onNoSessionsActive += OnNoSessionFound;
        NetworkManager.onSessionCreated += CreateSessions;
        NetworkManager.onLocalPlayerJoined += UpdatePlayerCountInSession;
        NetworkManager.onHostCheck += SetStartButton;
        NetworkManager.onSessionStartSucceeded += ShowInLobbyUi;
        CharacterSelectionNetworkManager.onLobbyPlayersChanged += RebuildLobbyPlayers;
        SessionInfoListUiItem.onSessionJoin += JoinIn;
        AddSearchDropdownListeners();
        AddCreateSettingsListeners();
        SetupCreateControls();
    }

    private void OnDisable()
    {
        NetworkManager.onNoSessionsActive -= OnNoSessionFound;
        NetworkManager.onSessionCreated -= CreateSessions;
        NetworkManager.onLocalPlayerJoined -= UpdatePlayerCountInSession;
        NetworkManager.onHostCheck -= SetStartButton;
        NetworkManager.onSessionStartSucceeded -= ShowInLobbyUi;
        CharacterSelectionNetworkManager.onLobbyPlayersChanged -= RebuildLobbyPlayers;
        SessionInfoListUiItem.onSessionJoin -= JoinIn;
        RemoveSearchDropdownListeners();
        RemoveCreateSettingsListeners();
    }
    public void ClearList()
    {
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        statusText.gameObject.SetActive(false);
    }

    public void UpdateName(string lobbyName)
    {
        _lastLobbyName = lobbyName;
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        SessionInfoListUiItem addedSessionItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUiItem>();

        addedSessionItem.SetInformation(sessionInfo);
    }

    public void CreateSessions(List<SessionInfo> sessionList)
    {
        _latestSessions.Clear();
        _latestSessions.AddRange(sessionList);
        RebuildFilteredSessionList();
    }

    private void RebuildFilteredSessionList()
    {
        ClearList();
        int shownSessions = 0;

        foreach (SessionInfo sessionInfo in _latestSessions)
        {
            if (!ShouldShowSession(sessionInfo))
            {
                Debug.Log($"SessionListUiHandler: filtered out {SessionMetadata.GetDebugDescription(sessionInfo)}");
                continue;
            }

            AddToList(sessionInfo);
            shownSessions++;
        }

        if (shownSessions == 0)
            ShowNoSessionsStatus(_latestSessions.Count > 0);
        else
            statusText.gameObject.SetActive(false);
    }
    
    public void OnCreatePressed()
    {
        if (string.IsNullOrWhiteSpace(_lastName))
        {
            ErrorHandlerUi.ReportError("Enter a session name.");
            return;
        }

        if (IsPasswordProtected() && string.IsNullOrWhiteSpace(GetPassword()))
        {
            ErrorHandlerUi.ReportError("Enter a password.");
            return;
        }

        if (string.IsNullOrWhiteSpace(GetNickname()))
        {
            ErrorHandlerUi.ReportError("Enter your nickname.");
            return;
        }

        onSessionCreated?.Invoke(CreateCurrentSessionRequest());
    }

    private void NotifySessionSettingsChanged()
    {
        onSessionSettingsChanged?.Invoke(CreateCurrentSessionRequest());
    }

    public void JoinIn(SessionInfo sessionInfo)
    {
        if (sessionInfo == null)
            return;

        _selectedJoinSession = sessionInfo;

        if (sessionList != null)
            sessionList.SetActive(false);

        if (joinLobbyPanel != null)
            joinLobbyPanel.SetActive(true);

        RefreshJoinPasswordFields();
    }

    public void OnJoinLobbyPressed()
    {
        if (_selectedJoinSession == null)
        {
            ErrorHandlerUi.ReportError("Select a lobby first.");
            return;
        }

        string nickname = GetJoinNickname();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            ErrorHandlerUi.ReportError("Enter your nickname.");
            return;
        }

        if (!SessionMetadata.MatchesPassword(_selectedJoinSession, GetJoinPassword()))
        {
            ErrorHandlerUi.ReportError("Wrong password.");
            return;
        }

        NetworkManager.Instance?.SetLocalPlayerNickname(nickname);
        NetworkManager.Instance?.StartSession(_selectedJoinSession.Name);
    }

    public void OnJoinRandomPressed()
    {
        List<SessionInfo> matchingSessions = new List<SessionInfo>();

        for (int i = 0; i < _latestSessions.Count; i++)
        {
            SessionInfo sessionInfo = _latestSessions[i];

            if (CanJoinSession(sessionInfo) && ShouldShowSession(sessionInfo))
                matchingSessions.Add(sessionInfo);
        }

        if (matchingSessions.Count == 0)
        {
            ErrorHandlerUi.ReportError("No matching room found.");
            return;
        }

        JoinIn(matchingSessions[Random.Range(0, matchingSessions.Count)]);
    }

    private void ShowInLobbyUi()
    {
        if (sessionList != null)
            sessionList.SetActive(false);

        if (sessionCreateLabel != null)
            sessionCreateLabel.SetActive(false);

        if (playerCounterUi != null)
            playerCounterUi.SetActive(true);

        if (joinLobbyPanel != null)
            joinLobbyPanel.SetActive(false);

        RebuildLobbyPlayers();
    }

    public void HostStartGame()
    {
        onHostStartedGame?.Invoke();
    }

    public void HostCloseSession()
    {
        onHostClosedSession?.Invoke();
    }

    public void LeaveSession()
    {
        onLocalPlayerLeftSession?.Invoke();
    }

    public void BackToLobbySelection()
    {
        onBackToLobbySelection?.Invoke();
    }

    public void UpdatePlayerCountInSession(SessionInfo sessionInfo)
    {
        playerCountInLobby.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
        RebuildLobbyPlayers();
    }

    private void SetStartButton(bool isHost)
    {
        StartGameButton.SetActive(isHost);
        SetCreatorControlsVisible(isHost);
        SetSessionExitButtons(isHost);
    }

    private void SetSessionExitButtons(bool isHost)
    {
        if (closeSessionButton != null)
            closeSessionButton.SetActive(isHost);

        if (leaveSessionButton != null)
            leaveSessionButton.SetActive(!isHost);
    }

    private void SetCreatorControlsVisible(bool visible)
    {
        if (showInLobbyToggle != null)
            showInLobbyToggle.gameObject.SetActive(visible);
    }

    public void OnInputUpdated(string text)
    {
        _lastName = text;
    }

    public void OnNicknameUpdated(string text)
    {
        _lastNickname = text;
        NetworkManager.Instance?.SetLocalPlayerNickname(GetNickname());
    }

    public void OnPasswordUpdated(string text)
    {
    }

    public void OnSearchFilterUpdated()
    {
        RebuildFilteredSessionList();
    }

    private bool ShouldShowSession(SessionInfo sessionInfo)
    {
        if (!CanShowSession(sessionInfo))
            return false;

        if (!MatchesSelectedGameMode(sessionInfo))
            return false;

        string preferredMap = GetDropdownValue(preferredMapDropdown, SessionMetadata.AnyMap);

        if (!IsAnyFilterValue(preferredMap) && !SessionMetadata.HasValue(sessionInfo, SessionMetadata.MapKey, preferredMap))
            return false;

        return MatchesSelectedRegion(sessionInfo);
    }

    private static bool CanShowSession(SessionInfo sessionInfo)
    {
        return sessionInfo != null && sessionInfo.IsVisible;
    }

    private static bool CanJoinSession(SessionInfo sessionInfo)
    {
        return CanShowSession(sessionInfo) && sessionInfo.IsOpen && !SessionMetadata.IsStarted(sessionInfo) && sessionInfo.PlayerCount < sessionInfo.MaxPlayers;
    }

    private static string GetDropdownValue(TMP_Dropdown dropdown, string fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return fallback;

        if (dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return fallback;

        return dropdown.options[dropdown.value].text;
    }

    private bool MatchesSelectedGameMode(SessionInfo sessionInfo)
    {
        string selectedGameMode = GetDropdownValue(searchGameModeDropdown, SessionMetadata.AnyMap);

        return IsAnyFilterValue(selectedGameMode) || SessionMetadata.HasValue(sessionInfo, SessionMetadata.GameModeKey, selectedGameMode);
    }

    private bool MatchesSelectedRegion(SessionInfo sessionInfo)
    {
        string selectedRegion = GetDropdownValue(searchRegionDropdown, SessionMetadata.AnyMap);

        return IsAnyFilterValue(selectedRegion) || SessionMetadata.HasValue(sessionInfo, SessionMetadata.RegionKey, selectedRegion);
    }

    private static bool IsAnyFilterValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) || value == SessionMetadata.AnyMap;
    }

    private void AddSearchDropdownListeners()
    {
        if (searchGameModeDropdown != null)
            searchGameModeDropdown.onValueChanged.AddListener(OnSearchDropdownValueChanged);

        if (preferredMapDropdown != null)
            preferredMapDropdown.onValueChanged.AddListener(OnSearchDropdownValueChanged);

        if (searchRegionDropdown != null)
            searchRegionDropdown.onValueChanged.AddListener(OnSearchDropdownValueChanged);
    }

    private void RemoveSearchDropdownListeners()
    {
        if (searchGameModeDropdown != null)
            searchGameModeDropdown.onValueChanged.RemoveListener(OnSearchDropdownValueChanged);

        if (preferredMapDropdown != null)
            preferredMapDropdown.onValueChanged.RemoveListener(OnSearchDropdownValueChanged);

        if (searchRegionDropdown != null)
            searchRegionDropdown.onValueChanged.RemoveListener(OnSearchDropdownValueChanged);
    }

    private void OnSearchDropdownValueChanged(int value)
    {
        OnSearchFilterUpdated();
    }

    private void AddCreateSettingsListeners()
    {
        if (showInLobbyToggle != null)
            showInLobbyToggle.onValueChanged.AddListener(OnShowInLobbyValueChanged);

        if (createGameModeDropdown != null)
            createGameModeDropdown.onValueChanged.AddListener(OnCreateDropdownValueChanged);

        if (createMapDropdown != null)
            createMapDropdown.onValueChanged.AddListener(OnCreateDropdownValueChanged);

        if (createRegionDropdown != null)
            createRegionDropdown.onValueChanged.AddListener(OnCreateDropdownValueChanged);

        if (passwordToggle != null)
            passwordToggle.onValueChanged.AddListener(OnPasswordToggleValueChanged);

        if (playerCountSlider != null)
            playerCountSlider.onValueChanged.AddListener(OnPlayerCountSliderValueChanged);
    }

    private void RemoveCreateSettingsListeners()
    {
        if (showInLobbyToggle != null)
            showInLobbyToggle.onValueChanged.RemoveListener(OnShowInLobbyValueChanged);

        if (createGameModeDropdown != null)
            createGameModeDropdown.onValueChanged.RemoveListener(OnCreateDropdownValueChanged);

        if (createMapDropdown != null)
            createMapDropdown.onValueChanged.RemoveListener(OnCreateDropdownValueChanged);

        if (createRegionDropdown != null)
            createRegionDropdown.onValueChanged.RemoveListener(OnCreateDropdownValueChanged);

        if (passwordToggle != null)
            passwordToggle.onValueChanged.RemoveListener(OnPasswordToggleValueChanged);

        if (playerCountSlider != null)
            playerCountSlider.onValueChanged.RemoveListener(OnPlayerCountSliderValueChanged);
    }

    private void OnShowInLobbyValueChanged(bool value)
    {
        NotifySessionSettingsChanged();
    }

    private void OnCreateDropdownValueChanged(int value)
    {
        NotifySessionSettingsChanged();
    }

    private void OnPasswordToggleValueChanged(bool value)
    {
        RefreshPasswordField();
    }

    private void OnPlayerCountSliderValueChanged(float value)
    {
        RefreshPlayerCountText();
    }

    private void SetupCreateControls()
    {
        if (playerCountSlider != null)
        {
            playerCountSlider.minValue = minPlayerCount;
            playerCountSlider.maxValue = maxPlayerCount;
            playerCountSlider.wholeNumbers = true;
            playerCountSlider.value = GetSelectedPlayerCount();
        }

        RefreshPasswordField();
        RefreshPlayerCountText();

        if (nicknameInputField != null)
            _lastNickname = nicknameInputField.text;

        NetworkManager.Instance?.SetLocalPlayerNickname(GetNickname());
    }

    private void RebuildLobbyPlayers()
    {
        ClearLobbyPlayers();

        if (playerDataPrefab == null || playersListParent == null)
            return;

        NetworkManager networkManager = NetworkManager.Instance;
        CharacterSelectionNetworkManager sessionManager = CharacterSelectionNetworkManager.Instance;

        if (networkManager == null || networkManager.Runner == null || sessionManager == null)
            return;

        bool localUserIsHost = networkManager.Runner.IsSharedModeMasterClient;
        PlayerRef localPlayer = networkManager.Runner.LocalPlayer;
        List<LobbyPlayerInfo> players = sessionManager.GetLobbyPlayers();

        for (int i = 0; i < players.Count; i++)
        {
            LobbyPlayerInfo player = players[i];
            PlayerDataUi row = Instantiate(playerDataPrefab, playersListParent);
            row.onKickRequested += OnKickPlayerRequested;
            row.SetData(player.Player, player.Nickname, player.IsHost, localUserIsHost && player.Player != localPlayer);
            _playerDataRows.Add(row);
        }
    }

    private void ClearLobbyPlayers()
    {
        for (int i = 0; i < _playerDataRows.Count; i++)
        {
            if (_playerDataRows[i] != null)
            {
                _playerDataRows[i].onKickRequested -= OnKickPlayerRequested;
                Destroy(_playerDataRows[i].gameObject);
            }
        }

        _playerDataRows.Clear();
    }

    private void OnKickPlayerRequested(PlayerRef player)
    {
        NetworkManager.Instance?.KickPlayer(player);
    }

    private void RefreshPasswordField()
    {
        if (passwordFieldObject != null)
            passwordFieldObject.SetActive(IsPasswordProtected());
        else if (passwordInputField != null)
            passwordInputField.gameObject.SetActive(IsPasswordProtected());
    }

    private void RefreshJoinPasswordFields()
    {
        bool requiresPassword = _selectedJoinSession != null && SessionMetadata.IsPasswordProtected(_selectedJoinSession);

        if (joinPasswordFieldObject != null)
            joinPasswordFieldObject.SetActive(requiresPassword);

        if (joinPasswordTitleObject != null)
            joinPasswordTitleObject.SetActive(requiresPassword);
    }

    private void RefreshPlayerCountText()
    {
        if (playerCountText != null)
            playerCountText.text = $"Size: {GetSelectedPlayerCount()}";
    }

    private SessionCreateRequest CreateCurrentSessionRequest()
    {
        return new SessionCreateRequest
        {
            SessionName = _lastName,
            ShowInLobby = showInLobbyToggle == null || showInLobbyToggle.isOn,
            GameMode = GetDropdownValue(createGameModeDropdown, "Default"),
            Map = GetDropdownValue(createMapDropdown, "Default"),
            Region = GetDropdownValue(createRegionDropdown, "Default"),
            PlayerCount = GetSelectedPlayerCount(),
            PasswordProtected = IsPasswordProtected(),
            Password = IsPasswordProtected() ? GetPassword() : string.Empty,
            PlayerNickname = GetNickname()
        };
    }

    private int GetSelectedPlayerCount()
    {
        float value = playerCountSlider == null ? maxPlayerCount : playerCountSlider.value;
        return Mathf.Clamp(Mathf.RoundToInt(value), minPlayerCount, maxPlayerCount);
    }

    private bool IsPasswordProtected()
    {
        return passwordToggle != null && passwordToggle.isOn;
    }

    private string GetPassword()
    {
        return passwordInputField == null ? string.Empty : passwordInputField.text;
    }

    private string GetNickname()
    {
        if (nicknameInputField != null)
            return nicknameInputField.text.Trim();

        return string.IsNullOrWhiteSpace(_lastNickname) ? string.Empty : _lastNickname.Trim();
    }

    private string GetJoinNickname()
    {
        return joinNicknameInputField == null ? string.Empty : joinNicknameInputField.text.Trim();
    }

    private string GetJoinPassword()
    {
        return joinPasswordInputField == null ? string.Empty : joinPasswordInputField.text;
    }

    public void OnNoSessionFound(bool state)
    {
        if (state)
        {
            statusText.text = "No sessions found.";
            statusText.gameObject.SetActive(true);
        }
        else
        {
            statusText.text = "Looking for sessions.";
            statusText.gameObject.SetActive(false);
        }
    }

    private void ShowNoSessionsStatus(bool filtered)
    {
        statusText.text = filtered ? "No matching sessions found." : "No sessions found.";
        statusText.gameObject.SetActive(true);
    }
}
