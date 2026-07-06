using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
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
    public GameObject StartGameButton;
    [SerializeField] private GameObject closeSessionButton;
    [SerializeField] private GameObject leaveSessionButton;
    public TMP_Text playerCountInLobby;
    [Header("Session Filters")]
    [SerializeField] private Toggle showInLobbyToggle;
    [SerializeField] private TMP_Dropdown createGameModeDropdown;
    [SerializeField] private TMP_Dropdown createMapDropdown;
    [SerializeField] private TMP_Dropdown searchGameModeDropdown;
    [SerializeField] private TMP_Dropdown preferredMapDropdown;
    [SerializeField] private float preferredMapFallbackSeconds = 5f;

    private string _lastName;
    private readonly List<SessionInfo> _latestSessions = new List<SessionInfo>();
    private Coroutine _preferredMapFallbackCoroutine;
    private bool _ignorePreferredMapFilter;

    public VerticalLayoutGroup verticalLayoutGroup;

    private string _lastLobbyName;

    private void OnEnable()
    {
        NetworkManager.onJoinedLobby += DisableLobbyList;
        NetworkManager.onNoSessionsActive += OnNoSessionFound;
        NetworkManager.onSessionCreated += CreateSessions;
        NetworkManager.onLocalPlayerJoined += UpdatePlayerCountInSession;
        NetworkManager.onHostCheck += SetStartButton;
        SessionInfoListUiItem.onSessionJoin += JoinIn;
        AddSearchDropdownListeners();
        AddCreateSettingsListeners();
    }

    private void OnDisable()
    {
        NetworkManager.onJoinedLobby -= DisableLobbyList;
        NetworkManager.onNoSessionsActive -= OnNoSessionFound;
        NetworkManager.onSessionCreated -= CreateSessions;
        NetworkManager.onLocalPlayerJoined -= UpdatePlayerCountInSession;
        NetworkManager.onHostCheck -= SetStartButton;
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

    private void DisableLobbyList()
    {
        LobbyList.SetActive(false);
        sessionList.SetActive(true);
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
        if (!string.IsNullOrWhiteSpace(_lastName))
        {
            onSessionCreated.Invoke(CreateCurrentSessionRequest());
        }
    }

    private void NotifySessionSettingsChanged()
    {
        onSessionSettingsChanged?.Invoke(CreateCurrentSessionRequest());
    }

    public void JoinIn(string a)
    {
        sessionList.SetActive(false);
        playerCounterUi.SetActive(true);
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

        if (createGameModeDropdown != null)
            createGameModeDropdown.gameObject.SetActive(visible);

        if (createMapDropdown != null)
            createMapDropdown.gameObject.SetActive(visible);
    }

    public void OnInputUpdated(string text)
    {
        _lastName = text;
    }

    public void OnSearchFilterUpdated()
    {
        _ignorePreferredMapFilter = false;

        if (_preferredMapFallbackCoroutine != null)
            StopCoroutine(_preferredMapFallbackCoroutine);

        _preferredMapFallbackCoroutine = StartCoroutine(DisablePreferredMapFilterAfterDelay());
        RebuildFilteredSessionList();
    }

    private IEnumerator DisablePreferredMapFilterAfterDelay()
    {
        yield return new WaitForSeconds(preferredMapFallbackSeconds);

        if (!HasPreferredMapMatch())
        {
            _ignorePreferredMapFilter = true;
            RebuildFilteredSessionList();
        }
    }

    private bool ShouldShowSession(SessionInfo sessionInfo)
    {
        if (!MatchesSelectedGameMode(sessionInfo))
            return false;

        string preferredMap = GetDropdownValue(preferredMapDropdown, SessionMetadata.AnyMap);

        if (!_ignorePreferredMapFilter && !string.IsNullOrEmpty(preferredMap) && preferredMap != SessionMetadata.AnyMap)
            return SessionMetadata.HasValue(sessionInfo, SessionMetadata.MapKey, preferredMap);

        return true;
    }

    private static string GetDropdownValue(TMP_Dropdown dropdown, string fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return fallback;

        if (dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return fallback;

        return dropdown.options[dropdown.value].text;
    }

    private bool HasPreferredMapMatch()
    {
        string preferredMap = GetDropdownValue(preferredMapDropdown, SessionMetadata.AnyMap);

        if (string.IsNullOrEmpty(preferredMap) || preferredMap == SessionMetadata.AnyMap)
            return true;

        foreach (SessionInfo sessionInfo in _latestSessions)
        {
            if (MatchesSelectedGameMode(sessionInfo) && SessionMetadata.HasValue(sessionInfo, SessionMetadata.MapKey, preferredMap))
                return true;
        }

        return false;
    }

    private bool MatchesSelectedGameMode(SessionInfo sessionInfo)
    {
        string selectedGameMode = GetDropdownValue(searchGameModeDropdown, string.Empty);

        return string.IsNullOrEmpty(selectedGameMode) || SessionMetadata.HasValue(sessionInfo, SessionMetadata.GameModeKey, selectedGameMode);
    }

    private void AddSearchDropdownListeners()
    {
        if (searchGameModeDropdown != null)
            searchGameModeDropdown.onValueChanged.AddListener(OnSearchDropdownValueChanged);

        if (preferredMapDropdown != null)
            preferredMapDropdown.onValueChanged.AddListener(OnSearchDropdownValueChanged);
    }

    private void RemoveSearchDropdownListeners()
    {
        if (searchGameModeDropdown != null)
            searchGameModeDropdown.onValueChanged.RemoveListener(OnSearchDropdownValueChanged);

        if (preferredMapDropdown != null)
            preferredMapDropdown.onValueChanged.RemoveListener(OnSearchDropdownValueChanged);
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
    }

    private void RemoveCreateSettingsListeners()
    {
        if (showInLobbyToggle != null)
            showInLobbyToggle.onValueChanged.RemoveListener(OnShowInLobbyValueChanged);

        if (createGameModeDropdown != null)
            createGameModeDropdown.onValueChanged.RemoveListener(OnCreateDropdownValueChanged);

        if (createMapDropdown != null)
            createMapDropdown.onValueChanged.RemoveListener(OnCreateDropdownValueChanged);
    }

    private void OnShowInLobbyValueChanged(bool value)
    {
        NotifySessionSettingsChanged();
    }

    private void OnCreateDropdownValueChanged(int value)
    {
        NotifySessionSettingsChanged();
    }

    private SessionCreateRequest CreateCurrentSessionRequest()
    {
        return new SessionCreateRequest
        {
            SessionName = _lastName,
            ShowInLobby = showInLobbyToggle == null || showInLobbyToggle.isOn,
            GameMode = GetDropdownValue(createGameModeDropdown, "Default"),
            Map = GetDropdownValue(createMapDropdown, "Default")
        };
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
