using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

enum AbilityState
{
    Ready,
    Casting,
    Cooldown
}


public class AbilityManager : NetworkBehaviour
{
    AbilityState abilityState = AbilityState.Ready;

    private Ability currentAbility;
    private Animator animator;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for AnimatedChampion");
        }
    }

    public void OnAnimationApex()
    {
        if (!IsOwner) return;

        currentAbility.OnUse(gameObject);
    }

    /// <summary>
    /// Casts the ability relevant to the parsed index. By calling the Ability's Activate() function
    /// </summary>
    /// <param name="_AbilityIndex"></param>
    public void TryCastAbility(Ability _abilty)
    {
        if (abilityState == AbilityState.Casting)
        {
            return;
        }
        currentAbility = _abilty;
        _abilty.Activate(gameObject, animator);
        StartCoroutine(LockCastingUntil(_abilty.CastTime));


    }

    private IEnumerator LockCastingUntil(float _timer)
    {
        abilityState = AbilityState.Casting;
        yield return new WaitForSeconds(_timer);
        abilityState = AbilityState.Ready;
    }


}
