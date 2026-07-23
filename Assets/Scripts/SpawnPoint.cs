using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform cameraPoint;

    public Transform CameraPoint => cameraPoint;
    public bool IsTaken { get; set; }
}
