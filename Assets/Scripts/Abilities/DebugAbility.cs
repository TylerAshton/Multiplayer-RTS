using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New DEBUG Ability", menuName = "Abilities/DEBUG")]
public class DebugAbility : Ability<IAbilityUser>
{
/*    public override Ability Clone()
    {
        DebugAbility clone = CreateInstance<DebugAbility>();
        CopyTo(clone);
        return clone;
    }*/

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
