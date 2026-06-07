using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageSendMechanism : MonoBehaviour
{
    [SerializeField] private TMP_InputField field;
    [SerializeField] private NetworkRunner networkRunner;
    

    public void SendMessage()
    {
        if (ChatManager.Instance == null)
        {
            Debug.LogError("ChatManager is not spawned yet");
            return;
        }

        ChatManager.Instance.RPC_SendMessageToServer(networkRunner.LocalPlayer, PlayerRef.None, field.text, true);
    }
}