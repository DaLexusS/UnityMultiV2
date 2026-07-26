using Fusion;
using UnityEngine;
using Object = System.Object;

public class PlayerAnimatorController : NetworkBehaviour
{
   
   [SerializeField] private NetworkMecanimAnimator networkAnimator;
   private Animator Animator;
   
   private static readonly int WALKING_BOOL = Animator.StringToHash("IsWalking");
   
   private static readonly int HIT_TRIGGER = Animator.StringToHash("WasHit");
   
   private static readonly int ATTACK_TRIGGER = Animator.StringToHash("ToThrow");
   
   private static readonly int DEATH_TRIGGER = Animator.StringToHash("WasKilled");
   public void SetAnimator(Animator animator)
   {
      Animator = animator;
      networkAnimator.Animator = Animator;
      Debug.LogWarning("Animator set");
   }

   public void SetWalkingAnimation(bool status)
   {
      Animator.SetBool(WALKING_BOOL, status);
   }


   public void ActivateHitAnimation()
   {
      if (!!Object.HasStateAuthority) return;
      
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
      Animator.SetTrigger(ATTACK_TRIGGER);
   }

   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   private void RPC_ActivateHitAnimation()
   {
      Animator.SetTrigger(HIT_TRIGGER);
   }

   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   private void RPC_ActivateDeathAnimation()
   {
      Animator.SetTrigger(DEATH_TRIGGER);
   }
   
   
}
