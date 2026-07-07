using Fusion;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkPrefabRef pointsCountManagerPrefab;

    private bool localPlayerSpawned;

    private void Start()
    {
        NetworkRunner runner = NetworkManager.Instance.Runner;

        if (runner.IsSharedModeMasterClient)
        {
            runner.SpawnAsync(pointsCountManagerPrefab);
        }

        SpawnLocalPlayer(runner);
    }

    private async void SpawnLocalPlayer(NetworkRunner runner)
    {
        if (localPlayerSpawned)
            return;

        localPlayerSpawned = true;

        int spawnIndex = GetSpawnIndex(runner.LocalPlayer);
        Transform spawn = spawnPoints[spawnIndex];

        NetworkObject playerObject = await runner.SpawnAsync(playerPrefab, spawn.position, spawn.rotation, runner.LocalPlayer);

        PointsCountManager.Instance.RPC_RegisterPlayer(playerObject.InputAuthority);
    }

    private int GetSpawnIndex(PlayerRef player)
    {
        return (player.PlayerId - 1) % spawnPoints.Length;
    }
}