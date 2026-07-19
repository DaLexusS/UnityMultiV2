using Fusion;
using UnityEngine;

public class CharacterObjectSpawner : NetworkBehaviour
{
    public NetworkObject objectPrefab;
    private NetworkObject spawnedNetworkObject;
    private GameObject spawnedLocalObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnObject();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DespawnObject();
        }
    }

    private void SpawnObject()
    {
        if (spawnedNetworkObject != null || spawnedLocalObject != null)
            return;

        Vector3 spawnPos = transform.position + transform.forward * 2f;

        if (Runner != null)
        {
            spawnedNetworkObject = Runner.Spawn(
                objectPrefab,
                spawnPos,
                Quaternion.identity,
                Object.InputAuthority
            );
        }
        else
        {
            spawnedLocalObject = Instantiate(
                objectPrefab.gameObject,
                spawnPos,
                Quaternion.identity
            );
        }
    }

    private void DespawnObject()
    {
        if (spawnedNetworkObject != null)
        {
            Runner.Despawn(spawnedNetworkObject);
            spawnedNetworkObject = null;
        }

        if (spawnedLocalObject != null)
        {
            Destroy(spawnedLocalObject);
            spawnedLocalObject = null;
        }
    }
}