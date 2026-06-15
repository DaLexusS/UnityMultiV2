using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    public CharacterSelectionState state;
    public TMPro.TMP_Text statusText;

    private static Dictionary<PlayerRef, int> playerSlots = new();

    private NetworkRunner _runner;

    NetworkRunner runner
    {
        get
        {
            if (_runner == null)
                _runner = FindFirstObjectByType<NetworkRunner>();

            return _runner;
        }
    }

    public void ChooseCharacter(int characterNumber)
    {
        PlayerRef player = runner.LocalPlayer;

        if (IsTaken(characterNumber))
        {
            statusText.text = "Character already taken!";
            return;
        }

        int index = GetOrAssignSlot(player);

        SetCharacter(index, characterNumber);

        statusText.text = $"Selected {characterNumber}";
    }

    public void SetReady()
    {
        int index = GetOrAssignSlot(runner.LocalPlayer);
        SetReady(index, 1);
    }

    int GetOrAssignSlot(PlayerRef player)
    {
        if (playerSlots.TryGetValue(player, out int index))
            return index;

        index = playerSlots.Count;
        playerSlots[player] = index;
        return index;
    }

    bool IsTaken(int character)
    {
        return state.Character0 == character ||
               state.Character1 == character ||
               state.Character2 == character ||
               state.Character3 == character ||
               state.Character4 == character ||
               state.Character5 == character ||
               state.Character6 == character ||
               state.Character7 == character ||
               state.Character8 == character ||
               state.Character9 == character;
    }

    void SetCharacter(int i, int value)
    {
        switch (i)
        {
            case 0: state.Character0 = value; break;
            case 1: state.Character1 = value; break;
            case 2: state.Character2 = value; break;
            case 3: state.Character3 = value; break;
            case 4: state.Character4 = value; break;
            case 5: state.Character5 = value; break;
            case 6: state.Character6 = value; break;
            case 7: state.Character7 = value; break;
            case 8: state.Character8 = value; break;
            case 9: state.Character9 = value; break;
        }
    }

    void SetReady(int i, int value)
    {
        switch (i)
        {
            case 0: state.Ready0 = value; break;
            case 1: state.Ready1 = value; break;
            case 2: state.Ready2 = value; break;
            case 3: state.Ready3 = value; break;
            case 4: state.Ready4 = value; break;
            case 5: state.Ready5 = value; break;
            case 6: state.Ready6 = value; break;
            case 7: state.Ready7 = value; break;
            case 8: state.Ready8 = value; break;
            case 9: state.Ready9 = value; break;
        }
    }
}