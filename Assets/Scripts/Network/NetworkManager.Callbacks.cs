using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public partial class NetworkManager
{
    public void OnGameStarted(NetworkRunner runner)
    {
        RefreshHostState(true, "Game started");

        if (networkRunner.IsSharedModeMasterClient)
        {
            NetworkObject chatManager = networkRunner.Spawn(chatManagerPrefab);
            DontDestroyOnLoad(chatManager.gameObject);

            NetworkObject selectionManager =
                networkRunner.Spawn(characterSelectionNetworkManager);
            DontDestroyOnLoad(selectionManager.gameObject);
        }

        SubmitLocalNickname();
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        SessionPlayerCountChanged?.Invoke(runner.SessionInfo);

        if (player == runner.LocalPlayer)
            SubmitLocalNickname();

        RefreshHostState(false, "Player joined");
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
        if (runner.IsSharedModeMasterClient)
            CharacterSelectionNetworkManager.Instance?.RemovePlayer(player);

        SessionPlayerCountChanged?.Invoke(runner.SessionInfo);
        RefreshHostState(true, "Player left");
    }

    void INetworkRunnerCallbacks.OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
        if (!_isIntentionalShutdown && shutdownReason != ShutdownReason.Ok)
            ReportServerError($"Network shutdown: {shutdownReason}");
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
        if (!_isIntentionalShutdown)
            ReportServerError($"Disconnected from server: {reason}");
    }

    void INetworkRunnerCallbacks.OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
        ReportServerError($"Connection failed: {reason}");
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
        SessionMetadata.LogSessionList(
            "NetworkManager.OnSessionListUpdated",
            sessionList
        );

        NoSessionsStateChanged?.Invoke(sessionList.Count == 0);
        SessionListUpdated?.Invoke(sessionList);
    }

    void INetworkRunnerCallbacks.OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
        RefreshHostState(true, "Host migration");
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        RefreshHostState(true, "Scene load done");
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject networkObject,
        PlayerRef player) { }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject networkObject,
        PlayerRef player) { }

    void INetworkRunnerCallbacks.OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token) { }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message) { }

    void INetworkRunnerCallbacks.OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ArraySegment<byte> data) { }

    void INetworkRunnerCallbacks.OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress) { }

    void INetworkRunnerCallbacks.OnInput(
        NetworkRunner runner,
        NetworkInput input) { }

    void INetworkRunnerCallbacks.OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input) { }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data) { }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
}
