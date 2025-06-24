using UnityEngine;

public class EmplacementBehaviour : UnitBehaviour
{
    private UnitTargettingManager targettingManager;
    public override void Init(Unit _unit)
    {
        base.Init(_unit);

        if (!TryGetComponent<UnitTargettingManager>(out targettingManager))
        {
            Debug.LogError($"{nameof(UnitTargettingManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
    }

    public override void Tick()
    {
        if (targettingManager.CurrentTarget != null)
        {
            unit.AbilityManager.TryCastAbility(0);
        }
    }
}
