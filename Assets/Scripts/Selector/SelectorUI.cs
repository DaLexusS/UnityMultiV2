using System.Collections.Generic;
using UnityEngine;

public class SelectorUI : MonoBehaviour
{
    private readonly List<PlayerItem> playerPanels = new();

    [SerializeField] private PlayerItem playerPanelPrefab;
    [SerializeField] private Transform selectorPanel;

    private void OnEnable()
    {
        CharacterSelectionNetworkManager
            .onLobbyPlayersChanged += UpdatePlayerList;

        ReadyManager.OnReadyStateChanged
            += RefreshPlayerPanels;

        UpdatePlayerList();
    }

    private void OnDisable()
    {
        CharacterSelectionNetworkManager
            .onLobbyPlayersChanged -= UpdatePlayerList;

        ReadyManager.OnReadyStateChanged
            -= RefreshPlayerPanels;
    }

    public void UpdatePlayerList()
    {
        ClearPlayerPanels();

        CharacterSelectionNetworkManager manager =
            CharacterSelectionNetworkManager.Instance;

        if (manager == null)
        {
            return;
        }

        List<LobbyPlayerInfo> players =
            manager.GetLobbyPlayers();

        for (int i = 0; i < players.Count; i++)
        {
            PlayerItem newPlayerPanel = Instantiate(
                playerPanelPrefab,
                selectorPanel,
                false
            );

            newPlayerPanel.Initialize(players[i]);

            newPlayerPanel.transform
                .SetSiblingIndex(i);

            playerPanels.Add(newPlayerPanel);
        }
    }

    private void RefreshPlayerPanels()
    {
        foreach (PlayerItem panel in playerPanels)
        {
            if (panel != null)
            {
                panel.RefreshState();
            }
        }
    }

    private void ClearPlayerPanels()
    {
        foreach (PlayerItem panel in playerPanels)
        {
            if (panel == null)
            {
                continue;
            }

            panel.gameObject.SetActive(false);
            Destroy(panel.gameObject);
        }

        playerPanels.Clear();
    }
}