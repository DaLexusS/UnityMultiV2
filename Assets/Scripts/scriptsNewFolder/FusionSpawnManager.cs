using Fusion;
using UnityEngine;

public class FusionSpawnManager : NetworkBehaviour
{
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private NetworkObject  playerPrefab;
    
    public override void Spawned()
    {
        base.Spawned();
        RPCRequestSpawn();
        
    }
    
    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawn(RpcInfo info = default)
    {
        int spawnSpawnIndex = 0;
        SpawnPoint targetSpawnPoint;
        do
        {
            spawnSpawnIndex = Random.Range(0, spawnPoints.Length);
            targetSpawnPoint = spawnPoints[spawnSpawnIndex];
        } while (targetSpawnPoint.isTaken);
    
        targetSpawnPoint.isTaken = true;
        RPCSetSpawnPoint(info.Source, spawnSpawnIndex);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCSetSpawnPoint([RpcTarget] PlayerRef targetPlayer, int spawnPointIndex)
    {
        
        Debug.Log("RPCSetSpawnPoint");
        SpawnPoint targetSpawnPoint = spawnPoints[spawnPointIndex];
    
        targetSpawnPoint.isTaken = true;
        Runner.SpawnAsync(playerPrefab, targetSpawnPoint.transform.position,
            targetSpawnPoint.transform.rotation);
    }
}