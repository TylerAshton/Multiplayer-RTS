using UnityEngine;

public class Unit : SelectableObject, IDestructible
{
    private Health health;
    public Health Health => health;
    private UnitBehaviour unitBehaviour;

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError("Health is required for Unit");
        }
    }

    protected override void Start()
    {
        base.Start();

        if (!IsServer)
        {
            return;
        }

        if (TryGetComponent<UnitBehaviour>(out unitBehaviour))
        {
            unitBehaviour.Init();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!IsServer)
        {
            return;
        }

        if (health.IsDying)
        {
            return;
        }

        if (unitBehaviour != null)
        {
            unitBehaviour.Tick();
        }


    }

    public virtual void DestroyObject()
    {
        if (rts_Player)
        {
            rts_Player.UnitManager.RemoveUnit(this);
        }
    }
}
