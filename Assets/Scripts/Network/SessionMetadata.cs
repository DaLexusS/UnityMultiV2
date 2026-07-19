using System.Collections.Generic;
using Fusion;
using UnityEngine;

public static class SessionMetadata
{
    public const string GameModeKey = "gameMode";
    public const string MapKey = "map";
    public const string RegionKey = "region";
    public const string StateKey = "state";
    public const string PasswordProtectedKey = "passwordProtected";
    public const string PasswordKey = "password";

    public const string StateLobby = "Lobby";
    public const string StateStarted = "Started";

    public const string AnyMap = "Any";

    public static Dictionary<string, SessionProperty> CreateProperties(string gameMode, string map, string state)
    {
        return CreateProperties(gameMode, map, "Default", state, false, string.Empty);
    }

    public static Dictionary<string, SessionProperty> CreateProperties(string gameMode, string map, string state, bool passwordProtected, string password)
    {
        return CreateProperties(gameMode, map, "Default", state, passwordProtected, password);
    }

    public static Dictionary<string, SessionProperty> CreateProperties(string gameMode, string map, string region, string state, bool passwordProtected, string password)
    {
        return new Dictionary<string, SessionProperty>
        {
            { GameModeKey, gameMode },
            { MapKey, map },
            { RegionKey, region },
            { StateKey, state },
            { PasswordProtectedKey, passwordProtected ? "true" : "false" },
            { PasswordKey, passwordProtected ? password : string.Empty }
        };
    }

    public static bool HasValue(SessionInfo sessionInfo, string key, string expectedValue)
    {
        return TryGetValue(sessionInfo, key, out string value) && value == expectedValue;
    }

    public static bool IsStarted(SessionInfo sessionInfo)
    {
        return HasValue(sessionInfo, StateKey, StateStarted);
    }

    public static bool IsPasswordProtected(SessionInfo sessionInfo)
    {
        return HasValue(sessionInfo, PasswordProtectedKey, "true");
    }

    public static bool MatchesPassword(SessionInfo sessionInfo, string password)
    {
        if (!IsPasswordProtected(sessionInfo))
            return true;

        return TryGetValue(sessionInfo, PasswordKey, out string expectedPassword) && expectedPassword == password;
    }

    public static bool TryGetValue(SessionInfo sessionInfo, string key, out string value)
    {
        value = string.Empty;

        if (sessionInfo.Properties == null || !sessionInfo.Properties.TryGetValue(key, out SessionProperty property))
            return false;

        value = property.PropertyValue == null ? string.Empty : property.PropertyValue.ToString();
        return !string.IsNullOrEmpty(value);
    }

    public static string GetDebugDescription(SessionInfo sessionInfo)
    {
        string gameMode = TryGetValue(sessionInfo, GameModeKey, out string gameModeValue) ? gameModeValue : "<missing>";
        string map = TryGetValue(sessionInfo, MapKey, out string mapValue) ? mapValue : "<missing>";
        string region = TryGetValue(sessionInfo, RegionKey, out string regionValue) ? regionValue : "<missing>";
        string state = TryGetValue(sessionInfo, StateKey, out string stateValue) ? stateValue : "<missing>";
        string passwordProtected = TryGetValue(sessionInfo, PasswordProtectedKey, out string passwordProtectedValue) ? passwordProtectedValue : "false";

        return $"{sessionInfo.Name} players={sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers} open={sessionInfo.IsOpen} visible={sessionInfo.IsVisible} gameMode={gameMode} map={map} region={region} state={state} passwordProtected={passwordProtected}";
    }

    public static void LogSessionList(string source, List<SessionInfo> sessions)
    {
        Debug.Log($"{source}: received {sessions.Count} sessions.");

        foreach (SessionInfo session in sessions)
            Debug.Log($"{source}: {GetDebugDescription(session)}");
    }
}
