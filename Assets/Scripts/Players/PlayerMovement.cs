using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.HasStateAuthority)
        {
            Vector3 movementVector = Vector3.zero;
            if (Keyboard.current.wKey.isPressed)
                movementVector = Vector3.forward;
            if (Keyboard.current.sKey.isPressed)
                movementVector = Vector3.back;
            if (Keyboard.current.aKey.isPressed)
                movementVector = Vector3.left;
            if (Keyboard.current.dKey.isPressed)
                movementVector = Vector3.right;
            
            transform.Translate(movementVector * speed * Runner.DeltaTime);
        }
    }
}