using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Spawn points")]
    [SerializeField] private SpawnPoint[] spawnPoints;

    [Header("Local camera")]
    [SerializeField] private Camera localCamera;

    [Header("Managers")]
    [SerializeField] private NetworkPrefabRef pointsCountManagerPrefab;

    private bool _localSpawnRequestSent;
    private bool _localPlayerSpawnInProgress;
    private bool _localPlayerSpawned;
    private bool _pointsManagerSpawnRequested;

    public override void Spawned()
    {
        if (Object.HasStateAuthority &&
            Runner.IsSharedModeMasterClient)
        {
            AsyncTaskRunner.Run(
                SpawnPointsCountManagerAsync(),
                this,
                "Could not create the score manager."
            );
        }

        AsyncTaskRunner.Run(
            RequestLocalPlayerSpawnAsync(),
            this,
            "Could not request the local player spawn."
        );
    }

    private async Task SpawnPointsCountManagerAsync()
    {
        if (_pointsManagerSpawnRequested)
            return;

        if (PointsCountManager.Instance != null)
            return;

        _pointsManagerSpawnRequested = true;

        try
        {
            await Runner.SpawnAsync(
                pointsCountManagerPrefab
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            _pointsManagerSpawnRequested = false;
        }
    }

    private async Task RequestLocalPlayerSpawnAsync()
    {
        if (_localSpawnRequestSent ||
            _localPlayerSpawned)
        {
            return;
        }
        
        while (ReadyManager.Instance == null)
        {
            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

        int confirmedSkinId =
            ReadyManager.Instance.GetConfirmedSkin(
                Runner.LocalPlayer
            );

        if (confirmedSkinId <= 0)
        {
            Debug.LogError(
                $"Player {Runner.LocalPlayer} has no confirmed skin.",
                this
            );

            return;
        }

        _localSpawnRequestSent = true;

        RPC_RequestSpawn();
    }

    /*
     * Any client can send this RPC,
     * but it executes only on State Authority.
     *
     * Because PlayerSpawner is a Master Client Object,
     * State Authority is the Master Client.
     */
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestSpawn(
        RpcInfo info = default)
    {
        if (!Object.HasStateAuthority ||
            !Runner.IsSharedModeMasterClient)
        {
            Debug.LogError(
                "Only the Master Client can choose spawn points.",
                this
            );

            return;
        }

        /*
         * Prevent duplicate player spawning.
         */
        if (Runner.TryGetPlayerObject(
                info.Source,
                out _))
        {
            Debug.LogWarning(
                $"Player {info.Source} already has a player object.",
                this
            );

            return;
        }

        int spawnPointIndex =
            GetRandomSpawnPointIndex();

        if (spawnPointIndex < 0)
        {
            Debug.LogError(
                "There are no free spawn points.",
                this
            );

            return;
        }

        /*
         * Only the master marks the position as occupied.
         */
        spawnPoints[spawnPointIndex].IsTaken = true;

        /*
         * Send the selected index only to
         * the player who requested the spawn.
         */
        RPC_AssignSpawnPoint(
            info.Source,
            spawnPointIndex
        );
    }

    private int GetRandomSpawnPointIndex()
    {
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return -1;
        }

        bool hasFreeSpawnPoint = false;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!spawnPoints[i].IsTaken)
            {
                hasFreeSpawnPoint = true;
                break;
            }
        }

        if (!hasFreeSpawnPoint)
        {
            return -1;
        }

        int index;

        do
        {
            index = UnityEngine.Random.Range(
                0,
                spawnPoints.Length
            );
        }
        while (spawnPoints[index].IsTaken);

        return index;
    }

    /*
     * Target RPC executes only on targetPlayer's client.
     */
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AssignSpawnPoint(
        [RpcTarget] PlayerRef targetPlayer,
        int spawnPointIndex)
    {
        if (targetPlayer != Runner.LocalPlayer)
        {
            return;
        }

        AsyncTaskRunner.Run(
            SpawnLocalPlayerAsync(spawnPointIndex),
            this,
            "Could not spawn the local player."
        );
    }

    private async Task SpawnLocalPlayerAsync(
        int spawnPointIndex)
    {
        if (_localPlayerSpawned ||
            _localPlayerSpawnInProgress)
        {
            return;
        }

        if (spawnPointIndex < 0 ||
            spawnPointIndex >= spawnPoints.Length)
        {
            Debug.LogError(
                $"Invalid spawn point index: {spawnPointIndex}",
                this
            );

            return;
        }

        _localPlayerSpawnInProgress = true;

        try
        {
            int confirmedSkinId =
                ReadyManager.Instance.GetConfirmedSkin(
                    Runner.LocalPlayer
                );

            if (confirmedSkinId <= 0)
            {
                Debug.LogError(
                    $"Player {Runner.LocalPlayer} has no confirmed skin.",
                    this
                );

                return;
            }

            SpawnPoint selectedSpawnPoint =
                spawnPoints[spawnPointIndex];

            NetworkObject playerObject =
                await Runner.SpawnAsync(
                    playerPrefab,
                    selectedSpawnPoint.transform.position,
                    selectedSpawnPoint.transform.rotation,
                    Runner.LocalPlayer,
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

            /*
             * Associate PlayerRef with its player object.
             */
            Runner.SetPlayerObject(
                Runner.LocalPlayer,
                playerObject
            );

            SetLocalCamera(selectedSpawnPoint);

            _localPlayerSpawned = true;

            AsyncTaskRunner.Run(
                RegisterPlayerWhenManagerIsReadyAsync(),
                this,
                "Could not register the local player for scoring."
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
        finally
        {
            _localPlayerSpawnInProgress = false;
        }
    }

    private void SetLocalCamera(
        SpawnPoint selectedSpawnPoint)
    {
        if (localCamera == null)
        {
            Debug.LogError(
                "Local camera is not assigned.",
                this
            );

            return;
        }

        Transform cameraPoint =
            selectedSpawnPoint.CameraPoint;

        if (cameraPoint == null)
        {
            Debug.LogError(
                $"Camera point is missing on {selectedSpawnPoint.name}.",
                selectedSpawnPoint
            );

            return;
        }

        localCamera.transform.SetPositionAndRotation(
            cameraPoint.position,
            cameraPoint.rotation
        );
    }

    private async Task RegisterPlayerWhenManagerIsReadyAsync()
    {
        const int maximumWaitFrames = 600;

        int waitedFrames = 0;

        while (PointsCountManager.Instance == null &&
               waitedFrames < maximumWaitFrames)
        {
            waitedFrames++;

            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

        if (PointsCountManager.Instance == null)
        {
            Debug.LogError(
                "PointsCountManager was not spawned.",
                this
            );

            return;
        }

        PointsCountManager.Instance.RPC_RegisterPlayer();
    }
    
    public void ExitTheGameToMenu()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.LeaveSession();
            return;
        }

        AsyncTaskRunner.Run(
            Runner.Shutdown(),
            this,
            "Could not leave the game."
        );
    }
}
