using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    public static ResultsUI Instance { get; private set; }
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    
    public void ShowResults()
    {
        resultsPanel.SetActive(true);
        resultsText.text = string.Empty;
        
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
