using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

enum AbilityState
{
    Ready,
    Casting,
    Cooldown
}

enum AbilityPositions
{
    Center,
    Firearm
}


public class AbilityManager : NetworkBehaviour
{
    AbilityState abilityState = AbilityState.Ready;

    private Ability currentAbility;
    private Animator animator;

    [SerializeField] private List<Transform> abilityPositions;
    public List<Transform>  AbilityPositions => abilityPositions;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for AbilityManager");
        }
    }

    public void OnAnimationApex()
    {
        if (!IsOwner) return;

        currentAbility.OnUse(gameObject, AbilityPositions);
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
