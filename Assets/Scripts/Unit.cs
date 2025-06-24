using UnityEngine;

public class Unit : SelectableObject, IDestructible
{
    Health health;
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

        if (TryGetComponent<UnitBehaviour>(out unitBehaviour))
        {
            unitBehaviour.Init(this);
        }

        
    }

    protected override void Update()
    {
        base.Update();

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
