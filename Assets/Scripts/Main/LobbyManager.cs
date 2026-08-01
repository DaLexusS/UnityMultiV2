public class LobbyManager
{
    private LobbyManagerSettings _config;

    public int LobbyCount => _config?.LobbyCount ?? 0;
    public int MaxPlayersPerLobby => _config?.MaxPlayersPerLobby ?? 0;

    public void Init(LobbyManagerSettings config)
    {
        _config = config;
    }
}
