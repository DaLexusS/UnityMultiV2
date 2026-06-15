using Fusion;
using UnityEngine;

public class CharacterSelectionNetworkManager : NetworkBehaviour
{
    [Networked, Capacity(10)]
    public NetworkArray<int> TakenCharacters => default;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestCharacter(int characterNumber, RpcInfo info = default)
    {
        int index = characterNumber - 1;

        if (TakenCharacters[index] == 0)
        {
            TakenCharacters.Set(index, info.Source.PlayerId);

            RPC_SelectionResult(info.Source, characterNumber, true);
        }
        else
        {
            RPC_SelectionResult(info.Source, characterNumber, false);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SelectionResult(PlayerRef targetPlayer, int characterNumber, bool approved)
    {
        if (Runner.LocalPlayer != targetPlayer)
            return;

        CharacterSelectUI ui = FindFirstObjectByType<CharacterSelectUI>();

        if (ui != null)
        {
            ui.ReceiveSelectionResult(characterNumber, approved);
        }
    }
}