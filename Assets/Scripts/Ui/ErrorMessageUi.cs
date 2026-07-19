using TMPro;
using UnityEngine;

public class ErrorMessageUi : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}
