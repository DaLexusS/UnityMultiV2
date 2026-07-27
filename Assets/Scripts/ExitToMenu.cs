using UnityEngine;

public class ExitToMenu : MonoBehaviour
{
    public void ExitToMenuMethod()
    {
        PointsCountManager.Instance.CloseRoomAfterResults();
    }
}
