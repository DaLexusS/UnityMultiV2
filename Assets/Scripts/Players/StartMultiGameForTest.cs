using Fusion;
using UnityEngine;

public class StartMultiGameForTest : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkRunner networkRunner;
    
    private int nextSpawnPoint = 0;
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
        SpawnLocalPlayer(runner);
    }

    private void SpawnLocalPlayer(NetworkRunner runner)
    {
        Transform spawn = GetNextSpawnPoint();

        runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation,
            runner.LocalPlayer
        );
    }

    private Transform GetNextSpawnPoint()
    {
        Transform point = spawnPoints[nextSpawnPoint];

        nextSpawnPoint++;

        if (nextSpawnPoint >= spawnPoints.Length)
            nextSpawnPoint = 0;

        return point;
    }
}
