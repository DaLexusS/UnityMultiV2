using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class Bomb : NetworkBehaviour
{
    [FormerlySerializedAs("flySpeed")]
    [SerializeField] private float _flySpeed;
    [FormerlySerializedAs("damage")]
    [SerializeField] private int _damage;
    [FormerlySerializedAs("visual")]
    [SerializeField] private GameObject _visual;
    [FormerlySerializedAs("explosionPartical")]
    [SerializeField] private ParticleSystem _explosionParticle;
    [FormerlySerializedAs("rb")]
    [SerializeField] private Rigidbody _rigidbody;
    
    [Networked] public PlayerRef Owner { get; private set; }
    public bool CanFly { get; set; }
    private Vector3 _flyDirection;
    private bool _destructionStarted;

    public override void Spawned()
    {
        _rigidbody.useGravity = false;
        _flyDirection = transform.forward;
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
            _rigidbody.linearVelocity = _flyDirection * _flySpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (!Object.HasStateAuthority || _destructionStarted)
            return;
        
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerHealth playerHealth))
        {
            if (playerHealth.Object.InputAuthority == Owner)
                return;

            playerHealth.RPCTakeDamage(_damage);
            PointsCountManager.Instance?.RPC_AddPoint();
            BeginDestruction();
        }
        
        else if (other.CompareTag("Wall"))
        {
            BeginDestruction();
        }
    }

    private void BeginDestruction()
    {
        if (_destructionStarted)
            return;

        _destructionStarted = true;
        CanFly = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        RPCPlayExplosion();
        _ = DespawnAfterExplosionAsync();
    }

    private async Task DespawnAfterExplosionAsync()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(2f, destroyCancellationToken);

            if (Object != null && Runner != null && Runner.IsRunning)
                Runner.Despawn(Object);
        }
        catch (OperationCanceledException)
        {
            // The projectile was already destroyed or the scene was unloaded.
        }
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCPlayExplosion()
    {
        _visual.SetActive(false);

        _explosionParticle.gameObject.SetActive(true);
        _explosionParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _explosionParticle.Play(true);
    }
    
}
