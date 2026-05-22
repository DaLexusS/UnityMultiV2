using UnityEngine;
using UnityEngine.Events;

public class LobbyItemHandler : MonoBehaviour
{
    public static UnityAction<string> onLobbyJoined;

    public string lobbyId;

    public void Join()
    {
        onLobbyJoined.Invoke(lobbyId);
    }
}
