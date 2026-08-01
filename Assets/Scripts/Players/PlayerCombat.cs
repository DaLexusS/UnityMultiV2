using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerAnimatorController _playerAnimator;
    [SerializeField] private Bomb bombPrefab;

    [SerializeField] private Transform bombSpawnPoint;
    [FormerlySerializedAs("coolDown")]
    [SerializeField] private float _cooldownSeconds = 1.5f;
    private bool _bombIsSpawned;
    private Bomb _currentBomb;

    public void Update()
    {
        if (!Object.HasStateAuthority) return;
        if (_bombIsSpawned) return;
        if (Keyboard.current == null) return;

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

    public void SpawnBomb()
    {
        AsyncTaskRunner.Run(
            SpawnBombAsync(),
            this,
            "Could not spawn the projectile."
        );
    }

    private async Task SpawnBombAsync()
    {
        if (!Object.HasStateAuthority) return;
        
        if (_bombIsSpawned) return;
        
        _bombIsSpawned = true;
        
        _currentBomb = Runner.Spawn(bombPrefab, bombSpawnPoint.position, transform.rotation);
        _currentBomb.Initialize(Object.InputAuthority);
        
        await Awaitable.WaitForSecondsAsync(_cooldownSeconds, destroyCancellationToken);
        
        _bombIsSpawned = false;
        _currentBomb = null;
    }
    
}
