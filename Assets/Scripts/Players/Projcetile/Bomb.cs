using System;
using Fusion;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float flySpeed;
    [SerializeField] private int damage;
    [SerializeField] private GameObject visual;
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
                    
                    BombDestruction(other.gameObject);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            if (HasStateAuthority)
            {
                BombDestruction(other.gameObject);
            }
            
        }
    }

    private async void BombDestruction(GameObject gameObject)
    {
        CanFly = false;
        RPCPlayExplosion();
        
        await Awaitable.WaitForSecondsAsync(2f);
        Runner.Despawn(Object);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCPlayExplosion()
    {
        visual.SetActive(false);

        explosionPartical.gameObject.SetActive(true);
        explosionPartical.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionPartical.Play(true);
    }
    
}
