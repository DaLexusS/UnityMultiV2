using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    private int MaxHp = 100;

    [Networked, OnChangedRender(nameof(CheckHealthAfterChanged))] [field: SerializeField]
    public int CurrentHp
    {
        get;
        set;
    }

    public override void Spawned()
    {
        base.Spawned();
        CurrentHp = MaxHp;
        PlayerUI.Instance.RegisterPlayer(Object.InputAuthority, CurrentHp);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        PlayerUI.Instance.UnRegisterPLayer(Object.InputAuthority);
    }

    [ContextMenu("Damage")]
    public void TakeDamage()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHp -= 10;
            CheckHealthAfterChanged();
        }
    }


    private void CheckHealthAfterChanged()
    {
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        PlayerUI.Instance.UpdateHealth(Object.InputAuthority, CurrentHp);
        
    }
    
}
