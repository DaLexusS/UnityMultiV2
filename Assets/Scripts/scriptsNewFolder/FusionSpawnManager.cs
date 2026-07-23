using Fusion;
using UnityEngine;

public class FusionSpawnManager : NetworkBehaviour
{
   // [SerializeField] private SpawnPoint[] spawnPoints;
   // [SerializeField] private NetworkObject  playerPrefab;
   // [SerializeField] private Camera localCamera;
   // 
   // public override void Spawned()
   // {
   //     base.Spawned();
   //     RPC_RequestSpawn();
   //     
   // }
//
   // [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
   // private void RPC_RequestSpawn(RpcInfo info = default)
   // {
   //     int spawnPointIndex = GetRandomSpawnPointIndex();
//
   //     if (spawnPointIndex == -1) return;
//
   //     SpawnPoint selectedPoint = spawnPoints[spawnPointIndex];
   //     selectedPoint.IsTaken = true;
   //     
   //     RPC_SetSpawnPoint(info.Source, spawnPointIndex);
   // }
//
   // private int GetRandomSpawnPointIndex()
   // {
   //     int index;
//
   //     do
   //     {
   //         index = Random.Range(0, spawnPoints.Length);
   //     }
   //     while (spawnPoints[index].IsTaken);
//
   //     return index;
   // }
   // 
   // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   // private void RPC_SetSpawnPoint(
   //     [RpcTarget] PlayerRef targetPlayer,
   //     int spawnPointIndex)
   // {
   //     if (spawnPointIndex < 0 ||
   //         spawnPointIndex >= spawnPoints.Length)
   //     {
   //         Debug.LogError(
   //             $"Invalid spawn point index: {spawnPointIndex}"
   //         );
//
   //         return;
   //     }
//
   //     SpawnPoint selectedSpawnPoint =
   //         spawnPoints[spawnPointIndex];
//
   //     SetLocalCamera(selectedSpawnPoint);
//
   //     NetworkObject playerObject = Runner.Spawn(
   //         playerPrefab,
   //         selectedSpawnPoint.transform.position,
   //         selectedSpawnPoint.transform.rotation
   //     );
//
   //     Runner.SetPlayerObject(
   //         Runner.LocalPlayer,
   //         playerObject
   //     );
   // }
//
   // private void SetLocalCamera(
   //     SpawnPoint selectedSpawnPoint)
   // {
   //     if (localCamera == null)
   //     {
   //         Debug.LogError(
   //             "Local camera is not assigned."
   //         );
//
   //         return;
   //     }
//
   //     Transform cameraPoint =
   //         selectedSpawnPoint.CameraPoint;
//
   //     if (cameraPoint == null)
   //     {
   //         Debug.LogError(
   //             $"Camera point is not assigned for " +
   //             $"{selectedSpawnPoint.name}."
   //         );
//
   //         return;
   //     }
//
   //     localCamera.transform.SetPositionAndRotation(
   //         cameraPoint.position,
   //         cameraPoint.rotation
   //     );
   // }
   // 
   // 
    
}