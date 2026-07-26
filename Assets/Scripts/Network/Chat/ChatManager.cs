using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    public override void Spawned()
    {
        Instance = this;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendMessageToServer(PlayerRef sender, PlayerRef targetPlayer, string message)
    {
        
        string nickname =
            CharacterSelectionNetworkManager.Instance
                .GetPlayerNickname(sender);
        
        if (targetPlayer == PlayerRef.None)
        {
            RPC_ReceiveMessageAll(nickname, message);
        }
        else
        {
            RPC_ReceiveMessagePersonal(targetPlayer, nickname, message);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessageAll(string nickname, string message)
    {
        ChatUIManager.Instance.AddMessage(nickname, message);
    }
    
   
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessagePersonal( [RpcTarget] PlayerRef targetPlayer, string nickname, string message)
    {
        ChatUIManager.Instance.AddMessage(nickname, message);
    }
}