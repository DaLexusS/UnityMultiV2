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
            Debug.LogError("ChatManager is not spawned yet");
            return;
        }

        PlayerRef target = _targetPlayerDropdown.TargetPlayer;
        PlayerRef sender = networkRunner.LocalPlayer;
        
        ChatManager.Instance.RPC_SendMessageToServer(sender, target , field.text);
    }
}