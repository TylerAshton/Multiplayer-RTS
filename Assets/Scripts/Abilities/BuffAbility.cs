using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Ability", menuName = "Abilities/Buff")]
public class BuffAbility : Ability
{
    [SerializeField] private GameObject buffEffects;
    [SerializeField] private Effect effect;
    public override void Activate(GameObject user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {

    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        GameObject buffVfx = Instantiate(buffEffects, _user.transform);
        buffVfx.GetComponent<NetworkObject>().Spawn();
        buffVfx.GetComponent<NetworkParent>().SetParent(_user.transform);
        _user.GetComponent<EffectManager>().AddEffect(effect);
    }
}
