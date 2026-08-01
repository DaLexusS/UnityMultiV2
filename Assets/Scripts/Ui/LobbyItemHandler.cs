using UnityEngine;
using UnityEngine.Events;

public class LobbyItemHandler : MonoBehaviour
{
    public static event UnityAction<string> LobbyJoinRequested;

    [SerializeField] private string lobbyId;

    public void Join()
    {
        LobbyJoinRequested.Invoke(lobbyId);
    }
}
