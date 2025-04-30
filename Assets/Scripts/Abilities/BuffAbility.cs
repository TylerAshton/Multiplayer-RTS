using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Ability", menuName = "Abilities/Buff")]
public class BuffAbility : Ability
{
    [SerializeField] private GameObject buffEffects;
    public override void Activate(GameObject user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {

    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        GameObject effect = Instantiate(buffEffects, _user.transform);
    }
}
