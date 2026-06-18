using Fusion;
using UnityEngine;

public class FusionSpawnManager : MonoBehaviour
{
    [SerializeField] private NetworkObject[] characterPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found!");
            return;
        }

        int selectedCharacter = PlayerData.SelectedCharacter;

        if (selectedCharacter < 1 || selectedCharacter > characterPrefabs.Length)
        {
            Debug.LogError("Wrong character index!");
            return;
        }

        int characterIndex = selectedCharacter - 1;

        NetworkObject selectedPrefab = characterPrefabs[characterIndex];

        int spawnIndex = (runner.LocalPlayer.PlayerId - 1) % spawnPoints.Length;
        Transform selectedSpawnPoint = spawnPoints[spawnIndex];

        runner.Spawn(
            selectedPrefab,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation,
            runner.LocalPlayer
        );
    }
}