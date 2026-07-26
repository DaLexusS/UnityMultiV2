using UnityEngine;

public class SkinAnimatorEventer : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerHealth playerHealth;

    public void Initialize(PlayerMovement movement, PlayerCombat combat, PlayerHealth health)
    {
        playerMovement = movement;
        playerCombat = combat;
        playerHealth = health;
    }

    public void EnableMovement()
    {
        playerMovement.EnableMovement();
    }

    public void SpawnBomb()
    {
        playerCombat.SpawnBomb();
    }

    public void AnimationDeathEvent()
    {
        playerHealth.AnimationDeathEvent();
    }
}
