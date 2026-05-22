using UnityEngine;
using UnityEditor.UI;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;
using UnityEngine.Events;

public class SessionInfoListUiItem : MonoBehaviour
{
    public static UnityAction<string> onSessionJoin;
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI sessionCountText;
    public Button joinButton;

    SessionInfo sessionInfo;
    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        sessionNameText.text = sessionInfo.Name;
        sessionCountText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        bool isJoinButtonActive = true;

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
        {
            isJoinButtonActive = false;
        }

        joinButton.gameObject.SetActive(isJoinButtonActive);
    }
    public void Join()
    {
        onSessionJoin.Invoke(sessionInfo.Name);
    }
}
