using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
   public static ChatUIManager Instance { get; private set; }

   [SerializeField] private TextMeshProUGUI textMessagePrefab;
   [SerializeField] private Transform content;
   
   [SerializeField] private ScrollRect scrollRect;
  

   private void Awake()
   {
      Instance = this;
   }

   public void AddMessage(PlayerRef sender, string message)
   {
      TextMeshProUGUI newTextMessage = Instantiate(textMessagePrefab, content);
      newTextMessage.text = $"{sender} : {message}";

      StartCoroutine(ScrollToBottom());
   }
   
   
   private IEnumerator ScrollToBottom()
   {
      yield return null;
      scrollRect.verticalNormalizedPosition = 0f;
   }
}
