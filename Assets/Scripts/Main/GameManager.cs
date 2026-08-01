using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LobbyManagerSettings lobbyManagerSettings;

    private LobbyManager _lobbyManager;

    private void Awake()
    {
        RunGame();        
    }
    private void RunGame()
    {
        _lobbyManager = new LobbyManager();
        _lobbyManager.Init(lobbyManagerSettings);
    }
}
