using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text readyStatusText;

    [Header("Buttons")]
    [SerializeField] private Button readyButton;

    
    [SerializeField] private Button[] skinButtons = new Button[ReadyManager.SkinCount];
    

    public PlayerRef Player { get; private set; }

    private bool initialized;

    private void Awake()
    {
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(HandleReadyClicked);
        }
        
        for (int i = 0; i < skinButtons.Length; i++)
        {
            if (skinButtons[i] == null)
            {
                continue;
            }

            int skinNumber = i + 1;

            skinButtons[i].onClick.AddListener(() => HandleSkinClicked(skinNumber)); 
        }
    }

    public void Initialize(LobbyPlayerInfo playerInfo)
    {
        Player = playerInfo.Player;
        initialized = true;
        nicknameText.text = playerInfo.Nickname;

        RefreshState();
    }

    public void RefreshState()
    {
        if (!initialized)
        {
            return;
        }

        CharacterSelectionNetworkManager lobbyManager = CharacterSelectionNetworkManager.Instance;

        ReadyManager readyManager = ReadyManager.Instance;

        if (lobbyManager == null || readyManager == null)
        {
            SetButtonsInteractable(false);
            return;
        }

        bool isLocalOwner = lobbyManager.Runner.LocalPlayer == Player;

        int selectedSkin = readyManager.GetSelectedSkin(Player);

        bool isReady = readyManager.IsPlayerReady(Player);

        for (int i = 0; i < skinButtons.Length; i++) 
        {
            int skinNumber = i + 1;

            bool lockedByAnotherPlayer = readyManager.IsSkinLockedByAnotherPlayer(skinNumber, Player);

            if (skinButtons[i] != null)
            {
               skinButtons[i].interactable = isLocalOwner && !isReady && !lockedByAnotherPlayer;
            }
        }

        bool hasSelectedSkin = selectedSkin >= 1 && selectedSkin <= ReadyManager.SkinCount;

        bool selectedSkinTakenByAnother = hasSelectedSkin && readyManager.IsSkinLockedByAnotherPlayer(selectedSkin, Player);

        if (readyButton != null)
        {
            readyButton.interactable = isLocalOwner && !isReady && hasSelectedSkin && !selectedSkinTakenByAnother;
        }

        UpdateReadyStatus(isReady, hasSelectedSkin, selectedSkinTakenByAnother);
    }

    private void HandleSkinClicked(int skinNumber)
    {
        if (!IsLocalPanelOwner())
        {
            return;
        }

        ReadyManager.Instance?.RPC_SelectSkin(skinNumber);
    }

    private void HandleReadyClicked()
    {
        if (!IsLocalPanelOwner())
        {
            return;
        }

        ReadyManager.Instance?.RPC_ReadyUp();
    }

    private bool IsLocalPanelOwner()
    {
        CharacterSelectionNetworkManager manager = CharacterSelectionNetworkManager.Instance;

        return manager != null && manager.Runner.LocalPlayer == Player;
    }

    private void UpdateReadyStatus(bool isReady, bool hasSelectedSkin, bool selectedSkinTakenByAnother)
    {
        if (readyStatusText == null)
        {
            return;
        }

        if (isReady)
        {
            readyStatusText.text = "READY";
            return;
        }

        if (!hasSelectedSkin)
        {
            readyStatusText.text = "CHOOSE A SKIN";
            return;
        }

        if (selectedSkinTakenByAnother)
        {
            readyStatusText.text = "SKIN TAKEN";
            return;
        }

        readyStatusText.text = "NOT READY";
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (readyButton != null)
        {
            readyButton.interactable = interactable;
        }

        foreach (Button skinButton in skinButtons)
        {
            if (skinButton != null)
            {
                skinButton.interactable = interactable;
            }
        }
    }
}