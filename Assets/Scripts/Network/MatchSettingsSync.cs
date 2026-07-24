using Fusion;
using UnityEngine;

public class MatchSettingsSync : NetworkBehaviour
{
    public static MatchSettingsSync Instance { get; private set; }

    public MatchSettingsJson CurrentSettings { get; private set; }

    public override void Spawned()
    {
        Instance = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    public void BroadcastSettings(MatchSettingsJson settings)
    {
        if (!Object.HasStateAuthority)
            return;

        string json = JsonUtility.ToJson(settings);
        RPC_ReceiveSettingsJson(json);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveSettingsJson(string json)
    {
        CurrentSettings = JsonUtility.FromJson<MatchSettingsJson>(json);

        Debug.Log($"Received match settings JSON: {json}");
    }
}
