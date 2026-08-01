using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f; 
    [SerializeField] private PlayerAnimatorController _playerAnimator;

    [Networked] public NetworkBool CanMove { get; private set; }
   
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
        if (Keyboard.current == null) return;
        
        
        Vector3 rotationVector = Vector3.zero;
        Vector3 movementVector = Vector3.zero;
        if (Keyboard.current.wKey.isPressed)
            movementVector = Vector3.forward;
        if (Keyboard.current.sKey.isPressed)
            movementVector = Vector3.back;
        if (Keyboard.current.aKey.isPressed)
            movementVector = Vector3.left;
        if (Keyboard.current.dKey.isPressed)
            movementVector = Vector3.right;
        if(Keyboard.current.qKey.isPressed)
            rotationVector += Vector3.down;
        if(Keyboard.current.eKey.isPressed)
            rotationVector += Vector3.up;
            
        transform.Rotate(rotationVector * (rotationSpeed * Runner.DeltaTime));
        transform.Translate(movementVector * speed * Runner.DeltaTime);
        
        
        _playerAnimator.SetWalkingAnimation(movementVector.sqrMagnitude > 0f);
    }
    
    
    public void StopMovement()
    {
        if (!Object.HasStateAuthority)
            return;

        CanMove = false;
        _playerAnimator.SetWalkingAnimation(false);
    }
    
    public void EnableMovement()
    {
        if (!Object.HasStateAuthority)
            return;

        CanMove = true;
    }
}
