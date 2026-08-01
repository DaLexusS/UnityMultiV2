public class SessionCreateRequest
{
    public string SessionName { get; set; }
    public bool ShowInLobby { get; set; }
    public string GameMode { get; set; }
    public string Map { get; set; }
    public string Region { get; set; }
    public int PlayerCount { get; set; }
    public bool PasswordProtected { get; set; }
    public string Password { get; set; }
    public string PlayerNickname { get; set; }
}
