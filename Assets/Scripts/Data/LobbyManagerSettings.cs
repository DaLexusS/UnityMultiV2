using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LobbyManagerSettings", menuName = "Scriptable Objects/LobbyManagerSettings")]
public class LobbyManagerSettings : ScriptableObject
{
    [Range(1, 8)]
    [FormerlySerializedAs("LobbyAmount")]
    [SerializeField] private int lobbyCount = 2;

    [Range(1, 10)]
    [FormerlySerializedAs("MaxPlayersInLobby")]
    [SerializeField] private int maxPlayersPerLobby = 10;

    public int LobbyCount => lobbyCount;
    public int MaxPlayersPerLobby => maxPlayersPerLobby;
}
