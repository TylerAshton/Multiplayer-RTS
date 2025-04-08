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
        EffectCoroutine(_abilityPositions[0]);
        GameObject newEffect = Instantiate(effect, _abilityPositions[0].position, Quaternion.identity);
        newEffect.transform.LookAt(_user.transform.forward);
    }

    private IEnumerator EffectCoroutine(Transform _effectPos)
    {
        GameObject newEffect = Instantiate(effect);
        yield return new WaitForSeconds(3);
        Destroy(newEffect);
    }
}
