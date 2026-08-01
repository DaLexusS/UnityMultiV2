using System;
using System.Collections;
using UnityEngine;

public class ErrorMessagePresenter : MonoBehaviour
{
    public static event Action<string> ErrorRequested;

    [SerializeField] private ErrorMessageView errorMessagePrefab;
    [SerializeField] private Transform messageParent;
    [SerializeField] private float visibleSeconds = 3f;
    [SerializeField] private string debugMessage = "Debug error test";

    private void OnEnable()
    {
        ErrorRequested += ShowMessage;
    }

    private void OnDisable()
    {
        ErrorRequested -= ShowMessage;
    }

    public static void ShowError(string message)
    {
        ErrorRequested?.Invoke(message);
    }

    public static void ReportError(string message)
    {
        string shownMessage = string.IsNullOrWhiteSpace(message) ? "Something went wrong." : message;
        Debug.LogError(shownMessage);
        ShowError(shownMessage);
    }

    public void ShowDebugMessage()
    {
        ReportError(debugMessage);
    }

    private void ShowMessage(string message)
    {
        if (errorMessagePrefab == null)
            return;

        Transform parent = messageParent != null ? messageParent : transform;
        ErrorMessageView errorMessage = Instantiate(errorMessagePrefab, parent);
        errorMessage.SetMessage(string.IsNullOrWhiteSpace(message) ? "Something went wrong." : message);

        StartCoroutine(HideAfterDelay(errorMessage.gameObject));
    }

    private IEnumerator HideAfterDelay(GameObject messageObject)
    {
        yield return new WaitForSeconds(visibleSeconds);

        if (messageObject != null)
            Destroy(messageObject);
    }
}
