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

        if (ReadyManager.Instance == null)
        {
            Debug.LogError(
                "ReadyManager was not found in the gameplay scene.",
                this
            );

            return;
        }

        int confirmedSkinId =
            ReadyManager.Instance.GetConfirmedSkin(
                runner.LocalPlayer
            );

        if (confirmedSkinId <= 0)
        {
            Debug.LogError(
                $"Player {runner.LocalPlayer} has no confirmed skin.",
                this
            );

            return;
        }

        int spawnIndex =
            GetSpawnIndex(runner.LocalPlayer);

        Transform spawn =
            spawnPoints[spawnIndex];

        NetworkObject playerObject =
            await runner.SpawnAsync(
                playerPrefab,
                spawn.position,
                spawn.rotation,
                runner.LocalPlayer,
                (spawnRunner, spawnedObject) =>
                {
                    PlayerSkinChanger skinChanger =
                        spawnedObject.GetComponent<PlayerSkinChanger>();

                    if (skinChanger == null)
                    {
                        Debug.LogError(
                            "PlayerSkinChanger was not found on the player prefab.",
                            spawnedObject
                        );

                        return;
                    }

                    skinChanger.SetInitialSkin(
                        confirmedSkinId
                    );
                }
            );

        localPlayerSpawned = true;

        PointsCountManager.Instance.RPC_RegisterPlayer(
            playerObject.InputAuthority
        );
    }

    private int GetSpawnIndex(PlayerRef player)
    {
        return (player.PlayerId - 1) % spawnPoints.Length;
    }
}