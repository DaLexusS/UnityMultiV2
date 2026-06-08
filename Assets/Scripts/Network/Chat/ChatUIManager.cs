using Fusion;
using TMPro;
using UnityEngine;

public class ChatUIManager : MonoBehaviour
{
   public static ChatUIManager Instance { get; private set; }
   
   
   [SerializeField] private TextMeshProUGUI messageField;

   private void Awake()
   {
      Instance = this;
   }

   public void AddMessage(PlayerRef sender, string message)
   {
      messageField.text = $"{sender}: {message}";
   }
}
