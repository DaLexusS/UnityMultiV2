using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }
    
    [SerializeField] private Slider[] hpBars;

    private Dictionary<PlayerRef, Slider> playerSliders = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerRef pl, int maxValue)
    {
        Slider sliderForNewPlayer = GetFreeSlider();
        playerSliders[pl] = sliderForNewPlayer;
        playerSliders[pl].maxValue = maxValue;
        UpdateHealth(pl, maxValue);
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
