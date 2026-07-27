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
    
    public void ShowResults()
    {
        resultsPanel.SetActive(true);
        
        var sortedResults = 
            PointsCountManager.Instance.GetResults().OrderByDescending(result => result.Value)
            .ThenBy(result => result.Key.PlayerId);

        int place = 1;

        foreach (var result in sortedResults)
        {
            string nickName = CharacterSelectionNetworkManager.Instance.GetPlayerNickname(result.Key);
            
            resultsText.text += $"{place}. {nickName} has {result.Value} points\n";

            place++;
        }
    }
}