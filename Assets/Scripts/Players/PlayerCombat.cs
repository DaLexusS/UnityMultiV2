using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private Bomb bombPrefab;

    [SerializeField] private Transform bombSpawnPoint;

    private bool bombIsSpawned;
    private Bomb currentbomb;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (!bombIsSpawned && Keyboard.current.spaceKey.isPressed)
        {
            PointsCountManager.Instance.RPC_AddPoint(Runner.LocalPlayer);
            currentbomb = Runner.Spawn(bombPrefab, bombSpawnPoint.position,  transform.rotation);
            bombIsSpawned = true;
        }

        if (bombIsSpawned && Mouse.current.leftButton.isPressed)
        {
            currentbomb.CanFly = true;
            bombIsSpawned = false;
            currentbomb = null;
        }
    }
}
