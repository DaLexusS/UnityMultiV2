using Fusion;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
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
      Animator.SetTrigger(HIT_TRIGGER);
   }
   public void ActivateAttackAnimation()
   {
      Animator.SetTrigger(ATTACK_TRIGGER);
   }
   public void ActivateDeathAnimation()
   {
      Animator.SetTrigger(DEATH_TRIGGER);
   }
   
   
}
