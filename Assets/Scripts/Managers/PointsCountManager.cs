using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class PointsCountManager : NetworkBehaviour
{
    public static PointsCountManager Instance { get; private set; }

    [Networked, Capacity(4)]
    private NetworkDictionary<PlayerRef, int> playerPoints => default;

    public override void Spawned()
    {
        Instance = this;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterPlayer(PlayerRef player)
    {
        if (!playerPoints.ContainsKey(player))
            playerPoints.Add(player, 0);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddPoint(PlayerRef player)
    {
        if (!playerPoints.ContainsKey(player))
            playerPoints.Add(player, 0);

        playerPoints.Set(player, playerPoints[player] + 1);
    }

    public NetworkDictionary<PlayerRef, int> GetResults()
    {
        return playerPoints;
    }
}
