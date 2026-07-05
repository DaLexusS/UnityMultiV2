using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.HasStateAuthority)
        {
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
        }
    }
}