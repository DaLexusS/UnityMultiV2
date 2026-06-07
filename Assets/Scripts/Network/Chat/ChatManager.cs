using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    public override void Spawned()
    {
        Instance = this;
        Debug.Log("ChatManager initialized by Fusion");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendMessageToServer(PlayerRef sender, PlayerRef targetPlayer, string message, bool sendToAll)
    {
        if (sendToAll)
        {
            RPC_ReceiveMessageAll(sender, message);
        }
        else
        {
            RPC_ReceiveMessagePersonal(targetPlayer, sender, message);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessageAll(PlayerRef sender, string message)
    {
        ChatUIManager.Instance.AddMessage(sender, message);
    }
    
   
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessagePersonal( [RpcTarget] PlayerRef targetPlayer, PlayerRef sender, string message)
    {
        Debug.Log($"{sender} says {message} to {targetPlayer}");
    }
}