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

    /// <summary>
    /// Called when the ability animation reaches the frame when the attack part of the ability should be cast. 
    /// Which then runs the currentAbility's OnUse function
    /// Requires the attack animation to have a correctly set up event that calls this function.
    /// </summary>
    public void OnAnimationApex()
    {
       if (!NetworkManager.Singleton.IsServer) return;

        currentAbility.OnUse(gameObject, AbilityPositions);
    }

    /// <summary>
    /// Casts the ability relevant to the parsed index. By calling the Ability's Activate() function
    /// </summary>
    /// <param name="_AbilityIndex"></param>
    public void TryCastAbility(Ability _abilty)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to cast an ability");
        }

        if (abilityState == AbilityState.Casting)
        {
            return;
        }
        currentAbility = _abilty;
        _abilty.Activate(gameObject, animator);
        StartCoroutine(LockCastingUntil(_abilty.CastTime));
    }

    /// <summary>
    /// Sets the AbilityState to Casting until the inputted time has elapsed.
    /// </summary>
    /// <param name="_timer"></param>
    /// <returns></returns>
    private IEnumerator LockCastingUntil(float _timer)
    {
        abilityState = AbilityState.Casting;
        yield return new WaitForSeconds(_timer);
        abilityState = AbilityState.Ready;
    }


}
