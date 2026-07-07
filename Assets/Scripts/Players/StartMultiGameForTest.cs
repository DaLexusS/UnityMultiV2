using Fusion;
using UnityEngine;

public class StartMultiGameForTest : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkRunner networkRunner;
    [SerializeField] private NetworkPrefabRef pointsCountManagerPrefan;

    private async void Start()
    {
        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Default",
            PlayerCount = 4,
            OnGameStarted = OnGameStarted
        });

        if (!result.Ok)
        {
            Debug.LogError(result.ShutdownReason);
        }
    }

    private void OnGameStarted(NetworkRunner runner)
    {
        if (runner.IsSharedModeMasterClient)
        {
            runner.Spawn(pointsCountManagerPrefan);
        }
        
        SpawnLocalPlayer(runner);
    }

    private void SpawnLocalPlayer(NetworkRunner runner)
    {
        int spawnIndex = GetSpawnIndex(runner.LocalPlayer);
        Transform spawn = spawnPoints[spawnIndex];

        NetworkObject playerObject =  runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation,
            runner.LocalPlayer
        );
        
        PointsCountManager.Instance.RPC_RegisterPlayer(playerObject.InputAuthority);
    }

    private int GetSpawnIndex(PlayerRef player)
    {
        return (player.PlayerId - 1) % spawnPoints.Length;
    }
}