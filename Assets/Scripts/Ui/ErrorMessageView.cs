using TMPro;
using UnityEngine;

public class ErrorMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}
