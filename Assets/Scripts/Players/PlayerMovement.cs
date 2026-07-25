using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f; 
    [SerializeField] private PlayerAnimatorController _playerAnimator;

    [Networked] public NetworkBool CanMove { get; private set; }
    private Quaternion movementRotation = Quaternion.identity;
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            CanMove = true;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (!Object.HasStateAuthority) return;
        if (!CanMove) return;
        
        
        float horizontal = 0f;
        float vertical = 0f;
        float rotation = 0f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.qKey.isPressed)
            rotation -= 1f;

        if (Keyboard.current.eKey.isPressed)
            rotation += 1f;

        Vector3 inputDirection =
            new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 movementDirection =
            movementRotation * inputDirection;

        transform.Rotate(
            Vector3.up,
            rotation * rotationSpeed * Runner.DeltaTime,
            Space.World
        );

        transform.position +=
            movementDirection * speed * Runner.DeltaTime;
        
        
        _playerAnimator.SetWalkingAnimation(movementDirection.sqrMagnitude > 0f);
    }
    
    
    public void StopMovement()
    {
        if (!Object.HasStateAuthority)
            return;

        CanMove = false;
        _playerAnimator.SetWalkingAnimation(false);
    }
}