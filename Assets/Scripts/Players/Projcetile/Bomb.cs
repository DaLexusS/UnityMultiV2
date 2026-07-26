using System;
using Fusion;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float flySpeed;
    [SerializeField] private int damage;
    [SerializeField] private GameObject visual;
    [SerializeField] private ParticleSystem explosionPartical;
    [SerializeField] private Rigidbody rb;
    
    [Networked] public PlayerRef Owner { get; private set; }
    public bool CanFly { get; set; }
    private Vector3 flyDirection;

    public override void Spawned()
    {
        rb.useGravity = false;
        flyDirection = transform.forward;
    }
    
    public void Initialize(PlayerRef owner)
    {
        if (!Object.HasStateAuthority)
            return;

        Owner = owner;
        CanFly = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && CanFly)
        {
            rb.linearVelocity = flyDirection * flySpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (!Object.HasStateAuthority)
            return;
        
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerHealth plHealth))
        {
            if (plHealth.Object.InputAuthority == Owner)
                return;

            plHealth.RPCTakeDamage(damage);
            plHealth.RPCTakeDamage(damage);
            PointsCountManager.Instance.RPC_AddPoint(Runner.LocalPlayer);
            BombDestruction(other.gameObject);
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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
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
