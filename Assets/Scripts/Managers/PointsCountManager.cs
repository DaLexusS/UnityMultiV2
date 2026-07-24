using System.Linq;
using Fusion;
using UnityEngine;

public class PointsCountManager : NetworkBehaviour
{
    public static PointsCountManager Instance { get; private set; }

    private const int MaxPlayers = 4;

    /*
     * 4 players: 3, 5, 8, 10
     * 3 players:    5, 8, 10
     * 2 players:       8, 10
     */
    private readonly int[] placementBonuses =
    {
        3,
        5,
        8,
        10
    };

    [Networked, Capacity(MaxPlayers)]
    private NetworkDictionary<PlayerRef, int> playerPoints => default;

    [Networked, Capacity(MaxPlayers)]
    private NetworkDictionary<PlayerRef, NetworkBool> alivePlayers => default;

    [Networked]
    private int TotalPlayers { get; set; }

    [Networked]
    private int AlivePlayersCount { get; set; }

    [Networked]
    private NetworkBool ResultsShown { get; set; }

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
        {
            TotalPlayers = Runner.ActivePlayers.Count();
        }
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterPlayer(
        PlayerRef player,
        RpcInfo info = default)
    {
        if (playerPoints.ContainsKey(player))
            return;

        playerPoints.Add(player, 0);
        alivePlayers.Add(player, true);

        AlivePlayersCount++;

        Debug.Log(
            $"{player} registered. " +
            $"Alive players: {AlivePlayersCount}"
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddPoint(PlayerRef player)
    {
        if (!playerPoints.ContainsKey(player))
            return;

        playerPoints.Set(
            player,
            playerPoints[player] + 1
        );

        Debug.Log(
            $"{player} received 1 point."
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDied(PlayerRef deadPlayer)
    {
        if (ResultsShown)
            return;

        if (!alivePlayers.TryGet(
                deadPlayer,
                out NetworkBool isAlive))
        {
            return;
        }
        
        if (!isAlive)
            return;

        alivePlayers.Set(deadPlayer, false);
        AlivePlayersCount--;

        AddDeathBonus(deadPlayer);

        Debug.Log(
            $"{deadPlayer} died. " +
            $"Alive players: {AlivePlayersCount}"
        );

        CheckGameEnd();
    }

    private void AddDeathBonus(PlayerRef deadPlayer)
    {
        int deathOrder = TotalPlayers - AlivePlayersCount;

        int firstBonusIndex = MaxPlayers - TotalPlayers;

        int bonusIndex = firstBonusIndex + deathOrder - 1;

        if (bonusIndex < 0 || bonusIndex >= placementBonuses.Length - 1) return;
        
        int bonus = placementBonuses[bonusIndex];

        AddPoints(deadPlayer, bonus);

        Debug.Log($"{deadPlayer} received death bonus: +{bonus}");
    }

    private void CheckGameEnd()
    {
        if (AlivePlayersCount > 1)
            return;
        
        if (AlivePlayersCount == 1)
        {
            PlayerRef survivor = FindAlivePlayer();

            if (survivor != PlayerRef.None)
            {
                AddPoints(survivor, 10);

                Debug.Log(
                    $"{survivor} survived and received +10 points."
                );
            }
        }
        
        ResultsShown = true;

        RPC_ShowResults();
        CloseRoomAfterResults();
    }

    private async void CloseRoomAfterResults()
    {
        if (!Object.HasStateAuthority ||
            !Runner.IsSharedModeMasterClient)
        {
            return;
        }

        await Awaitable.WaitForSecondsAsync(5f);

        NetworkManager.Instance?.CloseSessionForEveryone();
    }

    private PlayerRef FindAlivePlayer()
    {
        foreach (var player in alivePlayers)
        {
            if (player.Value)
            {
                return player.Key;
            }
        }

        return PlayerRef.None;
    }

    private void AddPoints(PlayerRef player, int amount)
    {
        if (!playerPoints.ContainsKey(player))
            return;

        playerPoints.Set(player, playerPoints[player] + amount);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowResults()
    {
        ResultsUI.Instance.ShowResults();
    }

    public NetworkDictionary<PlayerRef, int> GetResults()
    {
        return playerPoints;
    }

   
}
