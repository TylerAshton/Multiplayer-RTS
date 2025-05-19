using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Ability", menuName = "Abilities/Buff")]
public class BuffAbility : Ability<IAbilityUser>
{
    [SerializeField] private GameObject buffEffects;
    [SerializeField] private Effect effect;
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
        GameObject buffVfx = Instantiate(buffEffects, _user.Transform);
        buffVfx.GetComponent<NetworkObject>().Spawn();
        buffVfx.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        _user.EffectManager.AddEffect(effect);
    }
}
