using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerAnimatorController _playerAnimator;
    private int MaxHp = 100;

    [Networked, OnChangedRender(nameof(UpdateHealthUI))] [field: SerializeField]
    public int CurrentHp
    {
        get;
        set;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHp = MaxHp;
        }

        string nickname =
            CharacterSelectionNetworkManager.Instance != null
                ? CharacterSelectionNetworkManager.Instance
                    .GetPlayerNickname(Object.InputAuthority)
                : $"Player {Object.InputAuthority.PlayerId + 1}";

        PlayerUI.Instance?.RegisterPlayer(
            Object.InputAuthority,
            MaxHp,
            CurrentHp,
            nickname
        );
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        PlayerUI.Instance?.UnregisterPlayer(Object.InputAuthority);
        
    }

    [Rpc]
    public void RPCTakeDamage(int damage)
    {
        if (damage > 50)
            return;
        
        TakeDamage(damage);
    }

   
    private void TakeDamage(int damage)
    {
        if (Object.HasStateAuthority)
        {
            CurrentHp -= damage;
            CheckHealthAfterChanged();
            UpdateHealthUI();
        }
    }

    private void UpdateHealthUI()
    {
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        PlayerUI.Instance.UpdateHealth(Object.InputAuthority, CurrentHp);
    }


    private void CheckHealthAfterChanged()
    {
        _playerMovement.StopMovement();
        
        if (CurrentHp <= 0)
           Die();
        else 
            _playerAnimator.ActivateHitAnimation();
    }
    
    
    private void Die()
    {
        if (!Object.HasStateAuthority) return;
        
        PointsCountManager.Instance?.RPC_PlayerDied(Object.InputAuthority);
        
        _playerAnimator.ActivateDeathAnimation();
    }

    public void AnimationDeathEvent()
    {
        Runner.Despawn(Object);
    }
    
}
