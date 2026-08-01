using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageSendMechanism : MonoBehaviour
{
    [SerializeField] private TMP_InputField field;
    [SerializeField] private NetworkRunner networkRunner;
    [SerializeField] private TargetPlayerDropdown _targetPlayerDropdown;
    
    public void SendMessage()
    {
        if (ChatManager.Instance == null)
        {
            ErrorMessagePresenter.ShowError("Chat manager is not ready yet.");
            return;
        }

        PlayerRef target = _targetPlayerDropdown.TargetPlayer;
        ChatManager.Instance.RPC_SendMessageToServer(target, field.text);
        field.text = string.Empty;
    }
}
