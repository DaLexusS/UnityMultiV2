using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyPlayerRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject hostIndicator;
    [SerializeField] private Button kickButton;

    private PlayerRef _player;

    public event UnityAction<PlayerRef> KickRequested;

    private void Awake()
    {
        if (kickButton != null)
            kickButton.onClick.AddListener(OnKickPressed);
    }

    private void OnDestroy()
    {
        if (kickButton != null)
            kickButton.onClick.RemoveListener(OnKickPressed);
    }

    public void SetData(PlayerRef player, string nickname, bool isHost, bool canKick)
    {
        _player = player;

        if (playerNameText != null)
            playerNameText.text = nickname;

        if (hostIndicator != null)
            hostIndicator.SetActive(isHost);

        if (kickButton != null)
            kickButton.gameObject.SetActive(canKick);
    }

    private void OnKickPressed()
    {
        KickRequested?.Invoke(_player);
    }
}
