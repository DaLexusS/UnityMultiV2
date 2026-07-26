using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class CharacterSelectionNetworkManager : NetworkBehaviour
{
    public static CharacterSelectionNetworkManager Instance
    {
        get;
        private set;
    }

    public static UnityAction onLobbyPlayersChanged;

    [Networked, Capacity(4)]
    public NetworkArray<PlayerRef> LobbyPlayers => default;

    [Networked, Capacity(4)]
    public NetworkArray<NetworkString<_32>>
        LobbyPlayerNicknames => default;

    [Networked]
    private PlayerRef HostPlayer { get; set; }

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority &&
            HostPlayer == PlayerRef.None)
        {
            HostPlayer = Runner.LocalPlayer;
        }

        NetworkManager.Instance?.SubmitLocalNickname();

        onLobbyPlayersChanged?.Invoke();
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }

        onLobbyPlayersChanged?.Invoke();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterNickname(
        string nickname,
        RpcInfo info = default)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname =
                $"Player {info.Source.PlayerId + 1}";
        }

        nickname = nickname.Trim();

        int playerSlot = GetPlayerSlot(info.Source);

        bool isNewPlayer = playerSlot < 0;

        if (isNewPlayer)
        {
            playerSlot = GetEmptyPlayerSlot();
        }

        if (playerSlot < 0)
        {
            Debug.LogWarning(
                "The character selection lobby is full."
            );

            return;
        }

        /*
         * Clear stale selection data only for a genuinely new player.
         * Updating the nickname must not reset Ready state.
         */
        if (isNewPlayer)
        {
            ReadyManager.Instance?
                .ResetSlotForNewPlayer(playerSlot);
        }

        LobbyPlayers.Set(
            playerSlot,
            info.Source
        );

        LobbyPlayerNicknames.Set(
            playerSlot,
            nickname
        );

        RPC_LobbyPlayersChanged();
    }

    public void RemovePlayer(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        int playerSlot = GetPlayerSlot(player);

        if (playerSlot < 0)
        {
            return;
        }

        /*
         * Release the player's locked skin before
         * clearing their lobby slot.
         */
        ReadyManager.Instance?
            .RemovePlayerState(player, playerSlot);

        LobbyPlayers.Set(
            playerSlot,
            PlayerRef.None
        );

        LobbyPlayerNicknames.Set(
            playerSlot,
            string.Empty
        );

        RPC_LobbyPlayersChanged();

        /*
         * Example:
         * Three players exist, two are ready and the unready
         * player leaves. The remaining players are now all ready.
         */
        ReadyManager.Instance?
            .EvaluateAllPlayersReady();
    }

    public void RefreshMasterClientState()
    {
        if (!Object.HasStateAuthority ||
            !Runner.IsSharedModeMasterClient)
        {
            return;
        }

        bool stateChanged = false;

        if (HostPlayer != Runner.LocalPlayer)
        {
            HostPlayer = Runner.LocalPlayer;
            stateChanged = true;
        }

        List<PlayerRef> activePlayers =
            Runner.ActivePlayers.ToList();

        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            PlayerRef player = LobbyPlayers.Get(i);

            if (player == PlayerRef.None ||
                activePlayers.Contains(player))
            {
                continue;
            }

            ReadyManager.Instance?
                .RemovePlayerState(player, i);

            LobbyPlayers.Set(i, PlayerRef.None);
            LobbyPlayerNicknames.Set(i, string.Empty);
            stateChanged = true;
        }

        if (!stateChanged)
            return;

        RPC_LobbyPlayersChanged();
        ReadyManager.Instance?.EvaluateAllPlayersReady();
    }

    public List<LobbyPlayerInfo> GetLobbyPlayers()
    {
        List<LobbyPlayerInfo> players = new();

        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            PlayerRef player =
                LobbyPlayers.Get(i);

            if (player == PlayerRef.None)
            {
                continue;
            }

            string nickname =
                LobbyPlayerNicknames.Get(i).ToString();

            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname =
                    $"Player {player.PlayerId + 1}";
            }

            players.Add(new LobbyPlayerInfo
            {
                Player = player,
                Nickname = nickname,
                IsHost = player == HostPlayer
            });
        }

        return players
            .OrderBy(playerInfo =>
                playerInfo.Player.PlayerId)
            .ToList();
    }

    public int GetPlayerSlot(PlayerRef player)
    {
        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            if (LobbyPlayers.Get(i) == player)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetEmptyPlayerSlot()
    {
        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            if (LobbyPlayers.Get(i) == PlayerRef.None)
            {
                return i;
            }
        }

        return -1;
    }

    public void KickPlayer(PlayerRef targetPlayer)
    {
        if (!Runner.IsSharedModeMasterClient)
        {
            ErrorHandlerUi.ReportError(
                "Only the host can kick players."
            );

            return;
        }

        if (targetPlayer == Runner.LocalPlayer)
        {
            ErrorHandlerUi.ReportError(
                "Host cannot kick themself."
            );

            return;
        }

        RPC_KickedFromLobby(targetPlayer);

        RemovePlayer(targetPlayer);
        Runner.Disconnect(targetPlayer);

        ErrorHandlerUi.ReportError(
            "Player kicked from lobby."
        );
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LobbyPlayersChanged()
    {
        onLobbyPlayersChanged?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_KickedFromLobby(
        [RpcTarget] PlayerRef targetPlayer)
    {
        ErrorHandlerUi.ReportError(
            "You were kicked from the lobby."
        );

        NetworkManager.Instance?
            .LeaveSessionAfterDelay(1.5f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CloseSessionForEveryone()
    {
        NetworkManager networkManager =
            NetworkManager.Instance;

        if (networkManager == null)
        {
            return;
        }

        if (Runner.IsSharedModeMasterClient)
        {
            networkManager
                .LeaveSessionAfterDelay(0.5f);

            return;
        }

        networkManager.LeaveSession();
    }
    
    
    public string GetPlayerNickname(PlayerRef player)
    {
        int slot = GetPlayerSlot(player);

        if (slot < 0)
            return $"Player {player.PlayerId + 1}";

        string nickname =
            LobbyPlayerNicknames.Get(slot).ToString();

        if (string.IsNullOrWhiteSpace(nickname))
            return $"Player {player.PlayerId + 1}";

        return nickname;
    }
}
