using UnityEngine;
using System;
using System.Linq;
using Fusion;
using UnityEngine.SceneManagement;


public enum ReadyDeniedReason
{
    NoSkinSelected,
    SkinAlreadyTaken
}

public class ReadyManager : NetworkBehaviour
{
    public const int MaxPlayers = 4;
    public const int SkinCount = 4;

    public static ReadyManager Instance { get; private set; }

    public static event Action OnReadyStateChanged;

    [Header("Scene loading")]
    [SerializeField] private int gameplaySceneBuildIndex;
    
    [Networked, Capacity(MaxPlayers)] public NetworkArray<int> SelectedSkins => default;
    
    [Networked, Capacity(MaxPlayers)] public NetworkArray<int> ReadyStates => default;

    [Networked, Capacity(SkinCount)] public NetworkArray<int> LockedSkinOwners => default;

   
    [Networked, OnChangedRender(nameof(HandleRevisionChanged))] private int StateRevision { get; set; }

    [Networked] private NetworkBool SceneLoadStarted { get; set; }

    public override void Spawned()
    {
        Instance = this;

        // OnChangedRender is not called for initial values.
        OnReadyStateChanged?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }

        OnReadyStateChanged?.Invoke();
    }

    private void HandleRevisionChanged()
    {
        OnReadyStateChanged?.Invoke();
    }

    // Called by the local player after clicking a skin button.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SelectSkin(
        int skinNumber,
        RpcInfo info = default)
    {
        if (!IsValidSkin(skinNumber))
        {
            return;
        }

        CharacterSelectionNetworkManager lobbyManager =
            CharacterSelectionNetworkManager.Instance;

        if (lobbyManager == null)
        {
            return;
        }

        int playerSlot = lobbyManager.GetPlayerSlot(info.Source);

        if (!IsValidPlayerSlot(playerSlot))
        {
            return;
        }

        // Ready players cannot change their skin.
        if (IsReadyAtSlot(playerSlot))
        {
            return;
        }

        /*
         * A locked skin cannot even be provisionally selected.
         * This also protects the system from direct RPC calls.
         */
        if (IsSkinLockedByAnotherPlayerInternal(
                skinNumber,
                info.Source))
        {
            RPC_SkinSelectionDenied(info.Source);
            return;
        }

        if (SelectedSkins.Get(playerSlot) == skinNumber)
        {
            return;
        }

        SelectedSkins.Set(playerSlot, skinNumber);
        MarkStateChanged();
    }

    // Called by the local player after clicking Ready.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReadyUp(RpcInfo info = default)
    {
        CharacterSelectionNetworkManager lobbyManager =
            CharacterSelectionNetworkManager.Instance;

        if (lobbyManager == null)
        {
            return;
        }

        int playerSlot = lobbyManager.GetPlayerSlot(info.Source);

        if (!IsValidPlayerSlot(playerSlot))
        {
            return;
        }

        if (IsReadyAtSlot(playerSlot))
        {
            return;
        }

        int selectedSkin = SelectedSkins.Get(playerSlot);

        if (!IsValidSkin(selectedSkin))
        {
            RPC_ReadyDenied(
                info.Source,
                ReadyDeniedReason.NoSkinSelected
            );

            return;
        }

        int skinIndex = selectedSkin - 1;
        int currentSkinOwner = LockedSkinOwners.Get(skinIndex);
        int requestingPlayerKey = info.Source.RawEncoded;

        /*
         * Two players may have selected the same skin before
         * either of them pressed Ready.
         *
         * The first Ready request processed by StateAuthority wins.
         */
        if (currentSkinOwner != 0 &&
            currentSkinOwner != requestingPlayerKey)
        {
            RPC_ReadyDenied(
                info.Source,
                ReadyDeniedReason.SkinAlreadyTaken
            );

            /*
             * The selected skin stays selected visually.
             * Ready becomes disabled until another skin is selected.
             */
            MarkStateChanged();
            return;
        }

        LockedSkinOwners.Set(
            skinIndex,
            requestingPlayerKey
        );

        ReadyStates.Set(playerSlot, 1);

        MarkStateChanged();
        TryLoadGameplayScene();
    }

    public int GetSelectedSkin(PlayerRef player)
    {
        CharacterSelectionNetworkManager lobbyManager =
            CharacterSelectionNetworkManager.Instance;

        if (lobbyManager == null)
        {
            return 0;
        }

        int playerSlot = lobbyManager.GetPlayerSlot(player);

        if (!IsValidPlayerSlot(playerSlot))
        {
            return 0;
        }

        return SelectedSkins.Get(playerSlot);
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        CharacterSelectionNetworkManager lobbyManager =
            CharacterSelectionNetworkManager.Instance;

        if (lobbyManager == null)
        {
            return false;
        }

        int playerSlot = lobbyManager.GetPlayerSlot(player);

        return IsValidPlayerSlot(playerSlot) &&
               IsReadyAtSlot(playerSlot);
    }

    public bool IsSkinLocked(int skinNumber)
    {
        if (!IsValidSkin(skinNumber))
        {
            return true;
        }

        return LockedSkinOwners.Get(skinNumber - 1) != 0;
    }

    public bool IsSkinLockedByAnotherPlayer(
        int skinNumber,
        PlayerRef player)
    {
        if (!IsValidSkin(skinNumber))
        {
            return true;
        }

        return IsSkinLockedByAnotherPlayerInternal(
            skinNumber,
            player
        );
    }

    /*
     * Called when a new player occupies a previously empty lobby slot.
     * It prevents stale ready/skin data from a disconnected player.
     */
    public void ResetSlotForNewPlayer(int playerSlot)
    {
        if (!Object.HasStateAuthority ||
            !IsValidPlayerSlot(playerSlot))
        {
            return;
        }

        int oldSelectedSkin = SelectedSkins.Get(playerSlot);

        if (IsValidSkin(oldSelectedSkin))
        {
            LockedSkinOwners.Set(
                oldSelectedSkin - 1,
                0
            );
        }

        SelectedSkins.Set(playerSlot, 0);
        ReadyStates.Set(playerSlot, 0);

        MarkStateChanged();
    }

    /*
     * Must be called before or while removing the player
     * from LobbyPlayers.
     */
    public void RemovePlayerState(
        PlayerRef player,
        int playerSlot)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        int playerKey = player.RawEncoded;

        // Release every skin belonging to this player.
        for (int i = 0; i < LockedSkinOwners.Length; i++)
        {
            if (LockedSkinOwners.Get(i) == playerKey)
            {
                LockedSkinOwners.Set(i, 0);
            }
        }

        if (IsValidPlayerSlot(playerSlot))
        {
            SelectedSkins.Set(playerSlot, 0);
            ReadyStates.Set(playerSlot, 0);
        }

        MarkStateChanged();
    }

    /*
     * Called after LobbyPlayers has been updated.
     * For example, when an unready player leaves the lobby.
     */
    public void EvaluateAllPlayersReady()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        TryLoadGameplayScene();
    }

    private void TryLoadGameplayScene()
    {
        if (SceneLoadStarted)
        {
            return;
        }

        CharacterSelectionNetworkManager lobbyManager =
            CharacterSelectionNetworkManager.Instance;

        if (lobbyManager == null)
        {
            return;
        }

        int registeredPlayersCount = 0;

        for (int i = 0; i < lobbyManager.LobbyPlayers.Length; i++)
        {
            PlayerRef player =
                lobbyManager.LobbyPlayers.Get(i);

            if (player == PlayerRef.None)
            {
                continue;
            }

            registeredPlayersCount++;

            if (!IsReadyAtSlot(i))
            {
                return;
            }

            int selectedSkin = SelectedSkins.Get(i);

            if (!IsValidSkin(selectedSkin))
            {
                return;
            }

            /*
             * A ready player must really own
             * their selected skin.
             */
            int skinOwner =
                LockedSkinOwners.Get(selectedSkin - 1);

            if (skinOwner != player.RawEncoded)
            {
                return;
            }
        }

        if (registeredPlayersCount == 0)
        {
            return;
        }

        /*
         * Prevent the host from loading before a newly connected
         * player finishes nickname/lobby registration.
         */
        int activePlayersCount =
            Runner.ActivePlayers.Count();

        if (registeredPlayersCount != activePlayersCount)
        {
            return;
        }

        if (!Runner.IsSharedModeMasterClient)
        {
            Debug.LogError(
                "Only the Shared Mode Master Client can load the gameplay scene.",
                this
            );

            return;
        }

        if (gameplaySceneBuildIndex < 0 ||
            gameplaySceneBuildIndex >=
            SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError(
                $"Invalid gameplay scene build index: {gameplaySceneBuildIndex}",
                this
            );

            return;
        }

        SceneLoadStarted = true;
        MarkStateChanged();

        Runner.LoadScene(
            SceneRef.FromIndex(gameplaySceneBuildIndex),
            LoadSceneMode.Single
        );
    }

    private bool IsReadyAtSlot(int playerSlot)
    {
        return ReadyStates.Get(playerSlot) == 1;
    }

    private bool IsSkinLockedByAnotherPlayerInternal(
        int skinNumber,
        PlayerRef player)
    {
        int skinOwner =
            LockedSkinOwners.Get(skinNumber - 1);

        return skinOwner != 0 &&
               skinOwner != player.RawEncoded;
    }

    private void MarkStateChanged()
    {
        StateRevision++;
    }

    private static bool IsValidSkin(int skinNumber)
    {
        return skinNumber >= 1 &&
               skinNumber <= SkinCount;
    }

    private static bool IsValidPlayerSlot(int playerSlot)
    {
        return playerSlot >= 0 &&
               playerSlot < MaxPlayers;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SkinSelectionDenied(
        [RpcTarget] PlayerRef targetPlayer)
    {
        ErrorHandlerUi.ReportError(
            "This skin is already locked by another player."
        );
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReadyDenied(
        [RpcTarget] PlayerRef targetPlayer,
        ReadyDeniedReason reason)
    {
        switch (reason)
        {
            case ReadyDeniedReason.NoSkinSelected:
                ErrorHandlerUi.ReportError(
                    "Select a skin before pressing Ready."
                );
                break;

            case ReadyDeniedReason.SkinAlreadyTaken:
                ErrorHandlerUi.ReportError(
                    "This skin was locked by another player. Select another skin."
                );
                break;
        }

        OnReadyStateChanged?.Invoke();
    }
}
