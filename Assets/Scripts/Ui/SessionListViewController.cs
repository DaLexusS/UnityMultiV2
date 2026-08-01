using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class SessionListViewController : MonoBehaviour
{
    public static event UnityAction<SessionCreateRequest> SessionCreateRequested;
    public static event UnityAction<SessionCreateRequest> SessionSettingsChanged;
    public static event UnityAction HostStartRequested;
    public static event UnityAction HostCloseRequested;
    public static event UnityAction LocalPlayerLeaveRequested;
    public static event UnityAction BackToLobbyRequested;

    [FormerlySerializedAs("statusText")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [FormerlySerializedAs("sessionItemListPrefab")]
    [SerializeField] private GameObject _sessionItemPrefab;
    [FormerlySerializedAs("sessionList")]
    [SerializeField] private GameObject _sessionList;
    [FormerlySerializedAs("LobbyList")]
    [SerializeField] private GameObject _lobbyList;
    [FormerlySerializedAs("playerCounterUi")]
    [SerializeField] private GameObject _playerCounterUI;
    [SerializeField] private GameObject sessionCreateLabel;
    [SerializeField] private GameObject joinLobbyPanel;
    [FormerlySerializedAs("StartGameButton")]
    [SerializeField] private GameObject _startGameButton;
    [SerializeField] private GameObject closeSessionButton;
    [SerializeField] private GameObject leaveSessionButton;
    [SerializeField] private LobbyPlayerRowView playerDataPrefab;
    [SerializeField] private Transform playersListParent;
    [FormerlySerializedAs("playerCountInLobby")]
    [SerializeField] private TMP_Text _playerCountInLobbyText;
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
    [SerializeField] private int minPlayerCount = 3;
    [SerializeField] private int maxPlayerCount = 4;
    [SerializeField] private TMP_Dropdown searchGameModeDropdown;
    [SerializeField] private TMP_Dropdown preferredMapDropdown;
    [SerializeField] private TMP_Dropdown searchRegionDropdown;

    private string _lastName;
    private string _lastNickname;
    private SessionInfo _selectedJoinSession;
    private readonly List<SessionInfo> _latestSessions = new List<SessionInfo>();
    private readonly List<LobbyPlayerRowView> _playerDataRows = new List<LobbyPlayerRowView>();

    [FormerlySerializedAs("verticalLayoutGroup")]
    [SerializeField] private VerticalLayoutGroup _sessionListLayout;

    private string _lastLobbyName;

    private void OnEnable()
    {
        NetworkManager.NoSessionsStateChanged += OnNoSessionFound;
        NetworkManager.SessionListUpdated += CreateSessions;
        NetworkManager.SessionPlayerCountChanged += UpdatePlayerCountInSession;
        NetworkManager.HostStateChanged += SetStartButton;
        NetworkManager.SessionStartSucceeded += ShowInLobbyUi;
        CharacterSelectionNetworkManager.LobbyPlayersChanged += RebuildLobbyPlayers;
        SessionListItemView.SessionJoinRequested += JoinIn;
        AddSearchDropdownListeners();
        AddCreateSettingsListeners();
        SetupCreateControls();
    }

    private void OnDisable()
    {
        NetworkManager.NoSessionsStateChanged -= OnNoSessionFound;
        NetworkManager.SessionListUpdated -= CreateSessions;
        NetworkManager.SessionPlayerCountChanged -= UpdatePlayerCountInSession;
        NetworkManager.HostStateChanged -= SetStartButton;
        NetworkManager.SessionStartSucceeded -= ShowInLobbyUi;
        CharacterSelectionNetworkManager.LobbyPlayersChanged -= RebuildLobbyPlayers;
        SessionListItemView.SessionJoinRequested -= JoinIn;
        RemoveSearchDropdownListeners();
        RemoveCreateSettingsListeners();
    }
    public void ClearList()
    {
        foreach (Transform child in _sessionListLayout.transform)
        {
            Destroy(child.gameObject);
        }

        _statusText.gameObject.SetActive(false);
    }

    public void UpdateName(string lobbyName)
    {
        _lastLobbyName = lobbyName;
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        SessionListItemView addedSessionItem = Instantiate(_sessionItemPrefab, _sessionListLayout.transform).GetComponent<SessionListItemView>();

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
                Debug.Log($"SessionListViewController: filtered out {SessionMetadata.GetDebugDescription(sessionInfo)}");
                continue;
            }

            AddToList(sessionInfo);
            shownSessions++;
        }

        if (shownSessions == 0)
            ShowNoSessionsStatus(_latestSessions.Count > 0);
        else
            _statusText.gameObject.SetActive(false);
    }
    
    public void OnCreatePressed()
    {
        if (string.IsNullOrWhiteSpace(_lastName))
        {
            ErrorMessagePresenter.ShowError("Enter a session name.");
            return;
        }

        if (IsPasswordProtected() && string.IsNullOrWhiteSpace(GetPassword()))
        {
            ErrorMessagePresenter.ShowError("Enter a password.");
            return;
        }

        if (string.IsNullOrWhiteSpace(GetNickname()))
        {
            ErrorMessagePresenter.ShowError("Enter your nickname.");
            return;
        }

        SessionCreateRequested?.Invoke(CreateCurrentSessionRequest());
    }

    private void NotifySessionSettingsChanged()
    {
        SessionSettingsChanged?.Invoke(CreateCurrentSessionRequest());
    }

    public void JoinIn(SessionInfo sessionInfo)
    {
        if (sessionInfo == null)
            return;

        _selectedJoinSession = sessionInfo;

        if (_sessionList != null)
            _sessionList.SetActive(false);

        if (joinLobbyPanel != null)
            joinLobbyPanel.SetActive(true);

        RefreshJoinPasswordFields();
    }

    public void OnJoinLobbyPressed()
    {
        if (_selectedJoinSession == null)
        {
            ErrorMessagePresenter.ShowError("Select a lobby first.");
            return;
        }

        string nickname = GetJoinNickname();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            ErrorMessagePresenter.ShowError("Enter your nickname.");
            return;
        }

        if (!SessionMetadata.MatchesPassword(_selectedJoinSession, GetJoinPassword()))
        {
            ErrorMessagePresenter.ShowError("Wrong password.");
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
            ErrorMessagePresenter.ShowError("No matching room found.");
            return;
        }

        JoinIn(matchingSessions[Random.Range(0, matchingSessions.Count)]);
    }

    private void ShowInLobbyUi()
    {
        if (_sessionList != null)
            _sessionList.SetActive(false);

        if (sessionCreateLabel != null)
            sessionCreateLabel.SetActive(false);

        if (_playerCounterUI != null)
            _playerCounterUI.SetActive(true);

        if (joinLobbyPanel != null)
            joinLobbyPanel.SetActive(false);

        RebuildLobbyPlayers();
    }

    public void HostStartGame()
    {
        HostStartRequested?.Invoke();
    }

    public void HostCloseSession()
    {
        HostCloseRequested?.Invoke();
    }

    public void LeaveSession()
    {
        LocalPlayerLeaveRequested?.Invoke();
    }

    public void BackToLobbySelection()
    {
        BackToLobbyRequested?.Invoke();
    }

    public void UpdatePlayerCountInSession(SessionInfo sessionInfo)
    {
        _playerCountInLobbyText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
        RebuildLobbyPlayers();
    }

    private void SetStartButton(bool isHost)
    {
        _startGameButton.SetActive(isHost);
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
        return SessionBrowserFilter.Matches(
            sessionInfo,
            GetDropdownValue(searchGameModeDropdown, SessionMetadata.AnyMap),
            GetDropdownValue(preferredMapDropdown, SessionMetadata.AnyMap),
            GetDropdownValue(searchRegionDropdown, SessionMetadata.AnyMap)
        );
    }

    private static bool CanJoinSession(SessionInfo sessionInfo)
    {
        return SessionBrowserFilter.CanJoin(sessionInfo);
    }

    private static string GetDropdownValue(TMP_Dropdown dropdown, string fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return fallback;

        if (dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return fallback;

        return dropdown.options[dropdown.value].text;
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
            LobbyPlayerRowView row = Instantiate(playerDataPrefab, playersListParent);
            row.KickRequested += OnKickPlayerRequested;
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
                _playerDataRows[i].KickRequested -= OnKickPlayerRequested;
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
            _statusText.text = "No sessions found.";
            _statusText.gameObject.SetActive(true);
        }
        else
        {
            _statusText.text = "Looking for sessions.";
            _statusText.gameObject.SetActive(false);
        }
    }

    private void ShowNoSessionsStatus(bool filtered)
    {
        _statusText.text = filtered ? "No matching sessions found." : "No sessions found.";
        _statusText.gameObject.SetActive(true);
    }
}
