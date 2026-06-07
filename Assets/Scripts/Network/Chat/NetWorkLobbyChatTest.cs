using Fusion;
using UnityEngine;

public class NetWorkLobbyChatTest : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef chatManagerPrefab;
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
        if (runner.IsSharedModeMasterClient)
        {
            runner.Spawn(chatManagerPrefab);
        }
    }
}