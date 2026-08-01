using Fusion;
using System.Threading.Tasks;
using UnityEngine;

public class MultiplayerTestBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private NetworkRunner networkRunner;

    private void Start()
    {
        AsyncTaskRunner.Run(
            StartTestSessionAsync(),
            this,
            "Failed to start the test session."
        );
    }

    private async Task StartTestSessionAsync()
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
            ErrorMessagePresenter.ReportError($"Failed to start test session: {result.ShutdownReason}");
        }
    }

    private void OnGameStarted(NetworkRunner runner)
    {
        SpawnLocalPlayer(runner);
    }

    private void SpawnLocalPlayer(NetworkRunner runner)
    {
        int spawnIndex = GetSpawnIndex(runner.LocalPlayer);
        Transform spawn = spawnPoints[spawnIndex];

        runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation,
            runner.LocalPlayer
        );
    }

    private int GetSpawnIndex(PlayerRef player)
    {
        return (player.PlayerId - 1) % spawnPoints.Length;
    }
}
