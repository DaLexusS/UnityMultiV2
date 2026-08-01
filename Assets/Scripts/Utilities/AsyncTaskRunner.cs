using System;
using System.Threading.Tasks;
using UnityEngine;

public static class AsyncTaskRunner
{
    public static async void Run(
        Task task,
        UnityEngine.Object context,
        string userFacingError = null)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected when the owning object is destroyed or its scene unloads.
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, context);

            if (!string.IsNullOrWhiteSpace(userFacingError))
                ErrorMessagePresenter.ReportError(userFacingError);
        }
    }
}
