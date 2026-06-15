using UnityEngine;
using UnityEditor.UI;
using Fusion;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using NUnit.Framework;
using System.Collections.Generic;

public class SessionListUiHandler : MonoBehaviour
{
    public static UnityAction<string> onSessionCreated;
    public static UnityAction onHostStartedGame;

    public TextMeshProUGUI statusText;
    
    public GameObject sessionItemListPrefab;
    public GameObject sessionList;
    public GameObject LobbyList;
    public GameObject playerCounterUi;
    public GameObject StartGameButton;
    public TMP_Text playerCountInLobby;

    private string _lastName;

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
    }

    private void OnDisable()
    {
        NetworkManager.onJoinedLobby -= DisableLobbyList;
        NetworkManager.onNoSessionsActive -= OnNoSessionFound;
        NetworkManager.onSessionCreated -= CreateSessions;
        NetworkManager.onLocalPlayerJoined -= UpdatePlayerCountInSession;
        NetworkManager.onHostCheck -= SetStartButton;
        SessionInfoListUiItem.onSessionJoin -= JoinIn;
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
        ClearList();
        foreach (SessionInfo sessionInfo in sessionList)
        {
            AddToList(sessionInfo);
        }
    }
    
    public void OnCreatePressed()
    {
        if (_lastName != "")
        {
            onSessionCreated.Invoke(_lastName);
        }
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

    public void UpdatePlayerCountInSession(SessionInfo sessionInfo)
    {
        playerCountInLobby.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
    }

    private void SetStartButton(bool isHost)
    {
        StartGameButton.SetActive(isHost);
    }

    public void OnInputUpdated(string text)
    {
        _lastName = text;
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
}
