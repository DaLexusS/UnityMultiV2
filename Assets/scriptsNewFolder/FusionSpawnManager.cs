using Fusion;
using UnityEngine;

public class FusionSpawnManager : MonoBehaviour
{
    public NetworkObject[] characterPrefabs;
    public Transform[] spawnPoints;

    private void Start()
    {
        int selectedCharacter = PlayerData.SelectedCharacter;

        if (selectedCharacter < 1)
        {
            Debug.LogError("No character selected!");
            return;
        }

        int index = selectedCharacter - 1;
        NetworkObject selectedPrefab = characterPrefabs[index];
        Transform selectedSpawnPoint = spawnPoints[index];

        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();

        if (runner != null)
        {
            runner.Spawn(selectedPrefab, selectedSpawnPoint.position, Quaternion.identity, runner.LocalPlayer);
        }
        else
        {
            Instantiate(selectedPrefab.gameObject, selectedSpawnPoint.position, Quaternion.identity);
        }
    }
}