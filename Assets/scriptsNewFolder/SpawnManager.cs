using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform[] spawnPoints;

    private void Start()
    {
        int selectedCharacter = PlayerData.SelectedCharacter;

        if (selectedCharacter < 1)
        {
            Debug.LogError("No character selected!");
            return;
        }

        int spawnIndex = selectedCharacter - 1;

        Instantiate(
            characterPrefabs[selectedCharacter - 1],
            spawnPoints[spawnIndex].position,
            Quaternion.identity);
    }
}