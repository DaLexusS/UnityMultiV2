using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;
using UnityEngine.Events;

public class SessionListItemView : MonoBehaviour
{
    public static event UnityAction<SessionInfo> SessionJoinRequested;

    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionCountText;
    [SerializeField] private TextMeshProUGUI sessionStatusText;
    [SerializeField] private TextMeshProUGUI sessionModeText;
    [SerializeField] private TextMeshProUGUI sessionMapText;
    [SerializeField] private Button joinButton;

    private SessionInfo _sessionInfo;
    public void SetInformation(SessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;

        sessionNameText.text = sessionInfo.Name;
        sessionCountText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        if (sessionModeText != null)
            sessionModeText.text = $"Mode : {GetSessionProperty(sessionInfo, SessionMetadata.GameModeKey)}";

        if (sessionMapText != null)
            sessionMapText.text = $"Map : {GetSessionProperty(sessionInfo, SessionMetadata.MapKey)}";

        bool isFull = sessionInfo.PlayerCount >= sessionInfo.MaxPlayers;
        bool isStarted = SessionMetadata.IsStarted(sessionInfo);
        bool isJoinButtonActive = sessionInfo.IsOpen && !isFull && !isStarted;

        if (sessionStatusText != null)
            sessionStatusText.text = GetStatusText(sessionInfo, isFull, isStarted);

        joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    private static string GetStatusText(SessionInfo sessionInfo, bool isFull, bool isStarted)
    {
        if (isStarted)
            return "Started";

        if (!sessionInfo.IsOpen)
            return "Closed";

        if (isFull)
            return "Full";

        return "Waiting";
    }

    public void Join()
    {
        if (_sessionInfo == null ||
            !_sessionInfo.IsOpen ||
            SessionMetadata.IsStarted(_sessionInfo) ||
            _sessionInfo.PlayerCount >= _sessionInfo.MaxPlayers)
        {
            return;
        }

        SessionJoinRequested?.Invoke(_sessionInfo);
    }

    private static string GetSessionProperty(SessionInfo sessionInfo, string key)
    {
        return SessionMetadata.TryGetValue(sessionInfo, key, out string value) ? value : "Default";
    }
}
