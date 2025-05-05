using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AOE Ability", menuName = "Abilities/AOE")]
public class AOEAbility : Ability
{
    [SerializeField] GameObject effect;
    public override void Activate(GameObject user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {
        
    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        GameObject newEffect = Instantiate(effect, _abilityPositions[1]);
        newEffect.GetComponent<NetworkObject>().Spawn();
        newEffect.GetComponent<NetworkParent>().SetParent(_abilityPositions[1]);
    }
}
