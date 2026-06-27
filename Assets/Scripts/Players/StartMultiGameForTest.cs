using Fusion;
using UnityEngine;

public class StartMultiGameForTest : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private NetworkRunner networkRunner;
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
        networkRunner.Spawn(playerPrefab, spawnPoint.position);
    }
}
