using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
    public TMP_Text statusText;

    public void ChooseCharacter(int characterNumber)
    {
        PlayerData.SelectedCharacter = characterNumber;
        statusText.text = "Selected Character " + characterNumber;
    }

    public void ConfirmSelection()
    {
        if (PlayerData.SelectedCharacter == -1)
        {
            statusText.text = "Choose a character first!";
            return;
        }

        statusText.text = "Confirmed Character " + PlayerData.SelectedCharacter;

        SceneManager.LoadScene("GameScene");
    }
}