using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AOE Ability", menuName = "Abilities/AOE")]
public class AOEAbility : Ability
{
    [SerializeField] GameObject effect;
    public override void Activate(GameObject user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        //GameObject newEffect = Instantiate(effect, _abilityPositions[0].position, Quaternion.identity);
        GameObject newEffect = Instantiate(effect, _abilityPositions[1]);
        newEffect.transform.LookAt(_user.transform.forward);
    }

    
}
