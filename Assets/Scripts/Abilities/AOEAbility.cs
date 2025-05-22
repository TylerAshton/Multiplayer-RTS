using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AOE Ability", menuName = "Abilities/AOE")]
public class AOEAbility : Ability<IAbilityUser>
{
    [SerializeField] GameObject effect;
    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.Animator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject newEffect = Instantiate(effect, castPositionTransform);
        newEffect.GetComponent<NetworkObject>().Spawn();
        newEffect.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        newEffect.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }
}
