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

    protected Ability currentAbility;
    protected Animator animator;

    [SerializeField] private List<Transform> abilityPositions;
    public List<Transform>  AbilityPositions => new List<Transform>(abilityPositions);

    [SerializeField] protected List<Ability> abilities;
    public List<Ability> Abilities => new List<Ability>(abilities); // This prevents the list CONTENTS from being fucked with

    protected virtual void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for AbilityManager");
        }
    }

    protected void OnDrawGizmos()
    {
        #if UNITY_EDITOR
            if (abilities != null && abilities.Count > 0)
            {
                foreach (var ability in abilities)
                {
                    ability.DebugDrawing(gameObject, AbilityPositions);
                }
            }
            
        #endif
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
    public void TryCastAbility(int _abilityIndex)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to cast an ability");
            return;
        }

        if (abilityState == AbilityState.Casting)
        {
            return;
        }

        if (abilities[_abilityIndex] == null)
        {
            return;
        }
        currentAbility = abilities[_abilityIndex];
        currentAbility.Activate(gameObject, animator);
        StartCoroutine(LockCastingUntil(currentAbility.CastTime));
    }

    /// <summary>
    /// Sets the AbilityState to Casting until the inputted time has elapsed.
    /// </summary>
    /// <param name="_timer"></param>
    /// <returns></returns>
    protected IEnumerator LockCastingUntil(float _timer)
    {
        abilityState = AbilityState.Casting;
        yield return new WaitForSeconds(_timer);
        abilityState = AbilityState.Ready;
    }


}
