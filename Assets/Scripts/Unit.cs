using UnityEngine;

public class Unit : SelectableObject, IDestructible
{
    Health health;

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError("Health is required for Unit");
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
