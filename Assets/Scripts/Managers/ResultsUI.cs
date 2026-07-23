using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    public static ResultsUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI resultsText;
    
    private void Awake()
    {
        Instance = this;
    }
    

    [ContextMenu("Show me results")]
    public void ShowResults()
    {
        var sortedResults = 
            PointsCountManager.Instance.GetResults().OrderByDescending(result => result.Value)
            .ThenBy(result => result.Key.PlayerId);

        int place = 1;

        foreach (var result in sortedResults)
        {
            resultsText.text += $"{place}. {result.Key} has {result.Value} points\n";

            place++;
        }
    }
}