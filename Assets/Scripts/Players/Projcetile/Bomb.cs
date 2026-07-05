using System;
using Fusion;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float flySpeed;
    [SerializeField] private int damage;
    [SerializeField] private ParticleSystem explosionPartical;
    
    public bool CanFly { get; set; }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.HasStateAuthority && CanFly)
            transform.Translate(Vector3.forward * flySpeed * Runner.DeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (!Object.HasStateAuthority)
            return;
        
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerHealth plHealth))
        {
            PlayerHealth player = other.gameObject.GetComponent<PlayerHealth>();
            if (HasStateAuthority)
            {
                if (!player.HasStateAuthority)
                {
                    plHealth.RPCTakeDamage(damage);
                    Runner.Despawn(Object);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            if (HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
            
        }
    }
}
