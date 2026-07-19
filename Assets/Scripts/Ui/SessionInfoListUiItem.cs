using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;
using UnityEngine.Events;

public class SessionInfoListUiItem : MonoBehaviour
{
    public static UnityAction<SessionInfo> onSessionJoin;
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI sessionCountText;
    public TextMeshProUGUI sessionStatusText;
    [SerializeField] private TextMeshProUGUI sessionModeText;
    [SerializeField] private TextMeshProUGUI sessionMapText;
    public Button joinButton;

    SessionInfo sessionInfo;
    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

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
        if (sessionInfo == null || !sessionInfo.IsOpen || SessionMetadata.IsStarted(sessionInfo) || sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
            return;

        onSessionJoin?.Invoke(sessionInfo);
    }

    private static string GetSessionProperty(SessionInfo sessionInfo, string key)
    {
        return SessionMetadata.TryGetValue(sessionInfo, key, out string value) ? value : "Default";
    }
}
