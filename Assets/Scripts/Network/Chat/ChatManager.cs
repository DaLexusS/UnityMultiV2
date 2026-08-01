using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    public override void Spawned()
    {
        Instance = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendMessageToServer(
        PlayerRef targetPlayer,
        string message,
        RpcInfo info = default)
    {
        PlayerRef sender = info.Source;

        if (sender == PlayerRef.None ||
            !NetworkInputValidation.TryNormalizeChatMessage(
                message,
                out message
            ))
        {
            return;
        }

        CharacterSelectionNetworkManager playerManager =
            CharacterSelectionNetworkManager.Instance;

        if (playerManager == null || playerManager.GetPlayerSlot(sender) < 0)
            return;

        string nickname =
            playerManager.GetPlayerNickname(sender);
        
        if (targetPlayer == PlayerRef.None)
        {
            RPC_ReceiveMessageAll(nickname, message);
        }
        else
        {
            if (playerManager.GetPlayerSlot(targetPlayer) >= 0)
                RPC_ReceiveMessagePersonal(targetPlayer, nickname, message);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessageAll(string nickname, string message)
    {
        ChatUIManager.Instance?.AddMessage(nickname, message);
    }
    
   
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveMessagePersonal( [RpcTarget] PlayerRef targetPlayer, string nickname, string message)
    {
        ChatUIManager.Instance?.AddMessage(nickname, message);
    }
}
