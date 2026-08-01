using Fusion;

public static class SessionBrowserFilter
{
    public static bool CanJoin(SessionInfo session)
    {
        return CanShow(session) &&
               session.IsOpen &&
               !SessionMetadata.IsStarted(session) &&
               session.PlayerCount < session.MaxPlayers;
    }

    public static bool Matches(
        SessionInfo session,
        string gameMode,
        string map,
        string region)
    {
        if (!CanShow(session))
            return false;

        return MatchesProperty(
                   session,
                   SessionMetadata.GameModeKey,
                   gameMode
               ) &&
               MatchesProperty(
                   session,
                   SessionMetadata.MapKey,
                   map
               ) &&
               MatchesProperty(
                   session,
                   SessionMetadata.RegionKey,
                   region
               );
    }

    private static bool CanShow(SessionInfo session)
    {
        return session != null && session.IsVisible;
    }

    private static bool MatchesProperty(
        SessionInfo session,
        string key,
        string selectedValue)
    {
        return IsAny(selectedValue) ||
               SessionMetadata.HasValue(session, key, selectedValue);
    }

    private static bool IsAny(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
               value == SessionMetadata.AnyMap;
    }
}
