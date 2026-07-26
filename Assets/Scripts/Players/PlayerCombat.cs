using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerAnimatorController _playerAnimator;
    [SerializeField] private Bomb bombPrefab;

    [SerializeField] private Transform bombSpawnPoint;
    [SerializeField] private float coolDown = 1.5f;
    private bool bombIsSpawned;
    private Bomb currentbomb;

    public void Update()
    {
        if (!Object.HasStateAuthority) return;
        if (bombIsSpawned) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    private void Attack()
    {
        _playerAnimator.ActivateAttackAnimation();
        _playerMovement.StopMovement();
    }

    public async void SpawnBomb()
    {
        currentbomb = Runner.Spawn(bombPrefab, bombSpawnPoint.position,  transform.rotation);
        bombIsSpawned = true;
        currentbomb.CanFly = true;
        
        await Awaitable.WaitForSecondsAsync(coolDown);
        
        bombIsSpawned = false;
        currentbomb = null;
    }
    
}
