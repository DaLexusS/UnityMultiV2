using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private sealed class PlayerUIData 
    {
        public int MaxHealth { get; set; }
        public int CurrentHp { get; set; }
        public string Nickname { get; set; }
    }

    private readonly Dictionary<PlayerRef, PlayerUIData>
        registeredPlayers = new();

    private readonly Dictionary<PlayerRef, int>
        playerSlots = new();
    
    public static PlayerUI Instance { get; private set; }
    
    [SerializeField] private Slider[] hpBars;
    [SerializeField] private TextMeshProUGUI[] playerName;
    

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    

    public void RegisterPlayer(
        PlayerRef player,
        int maxHealth,
        int currentHp,
        string nickname)
    {
        registeredPlayers[player] = new PlayerUIData
        {
            MaxHealth = maxHealth,
            CurrentHp = currentHp,
            Nickname = nickname
        };

        RebuildUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        registeredPlayers.Remove(player);
        playerSlots.Remove(player);

        RebuildUI();
    }
    
    private void RebuildUI()
    {
        playerSlots.Clear();

        for (int i = 0; i < hpBars.Length; i++)
        {
            if (hpBars[i] != null)
                hpBars[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < playerName.Length; i++)
        {
            if (playerName[i] != null)
                playerName[i].gameObject.SetActive(false);
        }

        List<PlayerRef> sortedPlayers = registeredPlayers.Keys
            .OrderBy(player => player.PlayerId)
            .ToList();

        int availableSlots = Mathf.Min(
            hpBars.Length,
            playerName.Length
        );

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (i >= availableSlots)
                break;

            PlayerRef player = sortedPlayers[i];
            PlayerUIData data = registeredPlayers[player];

            Slider slider = hpBars[i];
            TextMeshProUGUI nicknameText = playerName[i];

            playerSlots[player] = i;

            slider.maxValue = data.MaxHealth;
            slider.value = data.CurrentHp;
            slider.gameObject.SetActive(true);

            nicknameText.text = data.Nickname;
            nicknameText.gameObject.SetActive(true);
        }
    }
    

    public void UpdateHealth(PlayerRef player, int value)
    {
        if (!registeredPlayers.TryGetValue(
                player,
                out PlayerUIData data))
        {
            return;
        }

        data.CurrentHp = value;

        if (!playerSlots.TryGetValue(
                player,
                out int slotIndex))
        {
            return;
        }

        if (slotIndex < 0 ||
            slotIndex >= hpBars.Length)
        {
            return;
        }

        hpBars[slotIndex].value = value;
    }
    
}
