using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] LobbyManagerSettings lobbyManagerSettings;

    private void Awake()
    {
        RunGame();        
    }
    private void RunGame()
    {
        LobbyManager lobbyManager = new LobbyManager();

        lobbyManager.Init(lobbyManagerSettings);
    }
}
