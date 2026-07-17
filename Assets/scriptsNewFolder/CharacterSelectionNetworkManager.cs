using Fusion;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class CharacterSelectionNetworkManager : NetworkBehaviour
{
    public static CharacterSelectionNetworkManager Instance { get; private set; }
    public static UnityAction onLobbyPlayersChanged;

    [Networked, Capacity(10)]
    public NetworkArray<int> TakenCharacters => default;

    [Networked, Capacity(4)]
    public NetworkArray<PlayerRef> LobbyPlayers => default;

    [Networked, Capacity(4)]
    public NetworkArray<NetworkString<_32>> LobbyPlayerNicknames => default;

    [Networked] private PlayerRef HostPlayer { get; set; }

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority && HostPlayer == PlayerRef.None)
            HostPlayer = Runner.LocalPlayer;

        NetworkManager.Instance?.SubmitLocalNickname();
        onLobbyPlayersChanged?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;

        onLobbyPlayersChanged?.Invoke();
    }

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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterNickname(string nickname, RpcInfo info = default)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            nickname = $"Player {info.Source.PlayerId}";

        int slot = GetPlayerSlot(info.Source);

        if (slot < 0)
            slot = GetEmptyPlayerSlot();

        if (slot < 0)
            return;

        LobbyPlayers.Set(slot, info.Source);
        LobbyPlayerNicknames.Set(slot, nickname);
        RPC_LobbyPlayersChanged();
    }

    public void RemovePlayer(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
            return;

        int slot = GetPlayerSlot(player);

        if (slot < 0)
            return;

        LobbyPlayers.Set(slot, PlayerRef.None);
        LobbyPlayerNicknames.Set(slot, string.Empty);
        RPC_LobbyPlayersChanged();
    }

    public List<LobbyPlayerInfo> GetLobbyPlayers()
    {
        List<LobbyPlayerInfo> players = new List<LobbyPlayerInfo>();

        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            PlayerRef player = LobbyPlayers.Get(i);

            if (player == PlayerRef.None)
                continue;

            players.Add(new LobbyPlayerInfo
            {
                Player = player,
                Nickname = LobbyPlayerNicknames.Get(i).ToString(),
                IsHost = player == HostPlayer
            });
        }

        return players;
    }

    public void KickPlayer(PlayerRef targetPlayer)
    {
        if (!Runner.IsSharedModeMasterClient)
        {
            ErrorHandlerUi.ReportError("Only the host can kick players.");
            return;
        }

        if (targetPlayer == Runner.LocalPlayer)
        {
            ErrorHandlerUi.ReportError("Host cannot kick themself.");
            return;
        }

        RPC_KickedFromLobby(targetPlayer);
        RemovePlayer(targetPlayer);
        Runner.Disconnect(targetPlayer);
        ErrorHandlerUi.ReportError("Player kicked from lobby.");
    }

    private int GetPlayerSlot(PlayerRef player)
    {
        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            if (LobbyPlayers.Get(i) == player)
                return i;
        }

        return -1;
    }

    private int GetEmptyPlayerSlot()
    {
        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            if (LobbyPlayers.Get(i) == PlayerRef.None)
                return i;
        }

        return -1;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LobbyPlayersChanged()
    {
        onLobbyPlayersChanged?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_KickedFromLobby([RpcTarget] PlayerRef targetPlayer)
    {
        ErrorHandlerUi.ReportError("You were kicked from the lobby.");
        NetworkManager.Instance?.LeaveSessionAfterDelay(1.5f);
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CloseSessionForEveryone()
    {
        NetworkManager networkManager = NetworkManager.Instance;

        if (networkManager == null)
            return;

        if (Runner.IsSharedModeMasterClient)
        {
            networkManager.LeaveSessionAfterDelay(0.5f);
            return;
        }

        networkManager.LeaveSession();
    }
}
