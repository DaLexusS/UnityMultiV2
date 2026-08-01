using Fusion;
using UnityEngine;

public class PlayerAnimatorController : NetworkBehaviour
{
   
   [SerializeField] private NetworkMecanimAnimator networkAnimator;
   private Animator _animator;
   
   private static readonly int WalkingBoolHash = Animator.StringToHash("IsWalking");
   
   private static readonly int HitTriggerHash = Animator.StringToHash("WasHit");
   
   private static readonly int AttackTriggerHash = Animator.StringToHash("ToThrow");
   
   private static readonly int DeathTriggerHash = Animator.StringToHash("WasKilled");
   public void SetAnimator(Animator animator)
   {
      _animator = animator;
      networkAnimator.Animator = _animator;
   }

   public void SetWalkingAnimation(bool status)
   {
      _animator.SetBool(WalkingBoolHash, status);
   }


   public void ActivateHitAnimation()
   {
       if (!Object.HasStateAuthority) return;
      
      RPC_ActivateHitAnimation();
   }
   
   public void ActivateAttackAnimation()
   {
      if (!Object.HasStateAuthority) return;
      
      RPC_ActivateAttackAnimation();
   }
   
   public void ActivateDeathAnimation()
   {
      if (!Object.HasStateAuthority) return;
      
      RPC_ActivateDeathAnimation();
   }
   
   
   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   private void RPC_ActivateAttackAnimation()
   {
      _animator.SetTrigger(AttackTriggerHash);
   }

   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   private void RPC_ActivateHitAnimation()
   {
      _animator.SetTrigger(HitTriggerHash);
   }

   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   private void RPC_ActivateDeathAnimation()
   {
      _animator.SetTrigger(DeathTriggerHash);
   }
   
   
}
