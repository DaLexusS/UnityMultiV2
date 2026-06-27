using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private NetworkRunner netRunner;
    public static PlayerUI Instance { get; private set; }
    
    [SerializeField] private Slider[] hpBars;

    private Dictionary<PlayerRef, Slider> playerSliders = new();

    private void Awake()
    {
        Instance = this;
    }
    
    public void RefreshPlayers()
    {
        playerSliders.Clear();

        foreach (Slider slider in hpBars)
            slider.gameObject.SetActive(false);

        List<PlayerRef> players = netRunner.ActivePlayers.OrderBy(player => player.PlayerId).ToList();

        for (int i = 0; i < players.Count; i++)
        {
            if (i >= hpBars.Length)
                break;

            PlayerRef player = players[i];
            Slider slider = hpBars[i];

            playerSliders[player] = slider;
            slider.gameObject.SetActive(true);
        }
    }

    public void RegisterPlayer(PlayerRef pl, int maxValue)
    {
        RefreshPlayers();
        
        playerSliders[pl].maxValue = maxValue;
        UpdateHealth(pl, maxValue);
    }

    public void UnRegisterPLayer(PlayerRef pl)
    {
        RefreshPlayers();
    }

    private Slider GetFreeSlider()
    {
        foreach (Slider slider in hpBars)
        {
            if (!playerSliders.ContainsValue(slider))
                return slider;
        }

        Debug.LogError("No free health sliders!");
        return null;
    }

    public void UpdateHealth(PlayerRef player, int value)
    {
        playerSliders[player].value = value;
    }
    
}
