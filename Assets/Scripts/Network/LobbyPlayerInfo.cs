using Fusion;

public struct LobbyPlayerInfo
{
    public PlayerRef Player { get; set; }
    public string Nickname { get; set; }
    public bool IsHost { get; set; }
}
