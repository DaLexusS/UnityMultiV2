using UnityEngine;

[CreateAssetMenu(fileName = "LobbyManagerSettings", menuName = "Scriptable Objects/LobbyManagerSettings")]
public class LobbyManagerSettings : ScriptableObject
{
    [Range(1, 8)]
    [SerializeField]
    private int LobbyAmount = 2;

    [Range(1, 10)]
    [SerializeField]
    private int MaxPlayersInLobby = 10;
}
