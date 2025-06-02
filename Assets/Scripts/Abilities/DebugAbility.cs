using UnityEngine;

[CreateAssetMenu(fileName = "New DEBUG Ability", menuName = "Abilities/DEBUG")]
public class DebugAbility : Ability<IAbilityUser>
{
    protected override void ActivateTyped(IAbilityUser _user)
    {
        Debug.Log(this.name);
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
    }
}
