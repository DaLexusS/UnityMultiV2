using Fusion;
using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultsText;

    [ContextMenu("Show me money")]
    public void ShowResults()
    {
        resultsText.text = "";

        foreach (var result in PointsCountManager.Instance.GetResults())
        {
            resultsText.text += $"{result.Key} has {result.Value} points\n";
        }
    }
}