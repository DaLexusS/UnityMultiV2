using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Fusion;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public TMP_Text statusText;
    public Button[] characterButtons;

    private int chosenCharacter = -1;

    private void Update()
    {
        if (FindFirstObjectByType<NetworkRunner>() != null)
        {
            RefreshTakenCharacters();
        }
    }

    public void ChooseCharacter(int characterNumber)
    {
        chosenCharacter = characterNumber;
        statusText.text = "Selected Character " + characterNumber;
    }

    public void ConfirmSelection()
    {
        if (chosenCharacter == -1)
        {
            statusText.text = "Choose a character first!";
            return;
        }

        // If Fusion is not running, test locally
        if (FindFirstObjectByType<NetworkRunner>() == null)
        {
            PlayerData.SelectedCharacter = chosenCharacter;
            SceneManager.LoadScene("GameScen");
            return;
        }

        CharacterSelectionNetworkManager manager =
            FindFirstObjectByType<CharacterSelectionNetworkManager>();

        if (manager == null)
        {
            statusText.text = "No selection manager found!";
            return;
        }

        statusText.text = "Requesting Character " + chosenCharacter + "...";
        manager.RPC_RequestCharacter(chosenCharacter);
    }

    public void ReceiveSelectionResult(int characterNumber, bool approved)
    {
        if (approved)
        {
            PlayerData.SelectedCharacter = characterNumber;
            statusText.text = "Approved Character " + characterNumber;

            SceneManager.LoadScene("GameScene");
        }
        else
        {
            statusText.text = "Character " + characterNumber + " is already occupied!";
        }
    }

    public void RefreshTakenCharacters()
    {
        CharacterSelectionNetworkManager manager =
            FindFirstObjectByType<CharacterSelectionNetworkManager>();

        if (manager == null)
            return;

        for (int i = 0; i < characterButtons.Length; i++)
        {
            bool isTaken = manager.TakenCharacters[i] != 0;
            characterButtons[i].interactable = !isTaken;
        }
    }
}