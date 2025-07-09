using UnityEngine;

public class EmplacementBehaviour : UnitBehaviour
{
    private UnitTargettingManager targettingManager;
    private IAbilityUser abilityUser;

    public override void Init()
    {
        base.Init();

        if (!TryGetComponent<UnitTargettingManager>(out targettingManager))
        {
            Debug.LogError($"{nameof(UnitTargettingManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
        if (!TryGetComponent<IAbilityUser>(out abilityUser))
        {
            Debug.LogError($"{nameof(IAbilityUser)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
    }

    public override void Tick()
    {
        if (abilityUser.CastTarget != null)
        {
            unit.AbilityManager.TryCastAbility(0);
        }
    }
}
