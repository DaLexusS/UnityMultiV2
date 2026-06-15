using System.Linq;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetPlayerDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private NetworkRunner netRunner;


    private List<PlayerRef> targets = new List<PlayerRef>();
    
    public PlayerRef TargetPlayer { get; private set; }
    
    
    private void OnEnable()
    {
        NetworkManager.onJoinedLobby += RefreshDropDownList;
    }

    private void OnDisable()
    {
        NetworkManager.onJoinedLobby -= RefreshDropDownList;
      
    }

    private void Start()
    {
        TargetPlayer = PlayerRef.None;
        dropdown.onValueChanged.AddListener(OnDropDownValueChange);
    }

    public void RefreshDropDownList()
    {
        targets.Clear(); 
        dropdown.ClearOptions();

        List<string> options = new List<string>() {"All players"};

        foreach (PlayerRef player in netRunner.ActivePlayers.OrderBy(p => p.PlayerId))
        {
            targets.Add(player);
            options.Add($"Player {targets.Count}");
        }
        
        dropdown.AddOptions(options);
        
        dropdown.value = 0;
        OnDropDownValueChange(dropdown.value);
    }

    private void OnDropDownValueChange(int index)
    {
        if (index == 0)
        {
            TargetPlayer = PlayerRef.None;
            return;
        }

        TargetPlayer = targets[index - 1];
        
    }
    
}
