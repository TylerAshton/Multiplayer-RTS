using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct AbilityPositionEntry // Work around struct for the dictionary of ability positions
{
    public AbilityPosition key;
    public Transform value;
}
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

    private IAbilityUser abilityUser;

    protected Ability currentAbility;
    protected Animator animator;

    [SerializeField] protected List<Ability> abilities;
    public List<Ability> Abilities => new List<Ability>(abilities); // This prevents the list CONTENTS from being fucked with

    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    private float AttackSpeed = 1;

    private NetworkObject networkObject;

    ulong ownerClientId = 999999;


    protected virtual void Awake()
    {
        if (!TryGetComponent<IAbilityUser>(out abilityUser))
        {
            Debug.LogError("AbilityUser is required for AbilityManager");
        }

        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for AbilityManager");
        }
        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("NetworkObject is required for AbilityManager");
        }
        
    }

    protected virtual void Start()
    {
        ownerClientId = networkObject.OwnerClientId;
    }

    protected void OnDrawGizmos()
    {
        #if UNITY_EDITOR
/*            if (abilities != null && abilities.Count > 0) TODO: Reenable me and fix nullRef for abilityUser
            {
                foreach (var ability in abilities)
                {
                    ability.DebugDrawing(abilityUser);
                }
            }*/
            
        #endif
    }

    public virtual void SetAbility(int _index, Ability _ability)
    {
        abilities[_index] = _ability;
    }

    //[Rpc(SendTo.Everyone)]
    //public void AddAbility(Ability _ability)
    //{
    //    AddAbilityRpc(_ability);
    //}

    public virtual void AddAbility(Ability _ability)
    {
        abilities.Add(_ability);
    }

    public bool CheckAbility(Ability _ability)
    {
        return abilities.Contains(_ability);
    }


    public void SetAttackSpeed(float _attackSpeed)
    {
        AttackSpeed = _attackSpeed;
        animator.SetFloat("AttackSpeed", _attackSpeed);
    }

    /// <summary>
    /// Called when the ability animation reaches the frame when the attack part of the ability should be cast. 
    /// Which then runs the currentAbility's OnUse function
    /// Requires the attack animation to have a correctly set up event that calls this function.
    /// </summary>
    public void OnAnimationApex()
    {
       if (!IsServer) return;

        currentAbility.OnUse(gameObject.GetComponent<IAbilityUser>());
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

        if (abilities[_abilityIndex] == null)
        {
            return;
        }

        currentAbility = abilities[_abilityIndex];

        if (!CanCastAbility(currentAbility))
        {
            Debug.LogWarning("Cannot cast ability due to checks failing");
            return;
        }

        StartCooldown(currentAbility);
        PointManager.Instance.RemovePoints(ownerClientId, currentAbility.AbilityCost);
        currentAbility.Activate(abilityUser);
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

    /// <summary>
    /// Checks if the ability can be used. Checking: if another ability is still casting, if player can afford it,
    /// and if the ability is on cooldown. 
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private bool CanCastAbility(Ability _ability)
    {
        // isCasting checker
        if (abilityState == AbilityState.Casting)
        {
            return false;
        }
        // Cost checker // TODO: Enable ability cost checking
        int currentPoints = PointManager.Instance.GetPoints(ownerClientId);

        if (currentPoints < _ability.AbilityCost)
        {
            return false;
        }

        // Cooldown checker
        if (cooldownTimers.TryGetValue(_ability.AbilityID, out float lastUsedTime))
        {
            if (Time.time < lastUsedTime + _ability.Cooldown)
            {
                return false;
            }
        }


        return true; 
    }

    /// <summary>
    /// Sets the cooldown for an ability in the server and sends an update to the clients
    /// </summary>
    /// <param name="_ability"></param>
    private void StartCooldown(Ability _ability)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to set an ability cooldown");
            return;
        }

        cooldownTimers[_ability.AbilityID] = Time.time;
        SetCooldownRpc(_ability.AbilityID, Time.time);
    }

    /// <summary>
    /// Updates the clients with the cooldown of said ability
    /// </summary>
    /// <param name="abilityID"></param>
    /// <param name="serverTimeStamp"></param>
    [Rpc(SendTo.NotMe)]
    private void SetCooldownRpc(string abilityID, float serverTimeStamp)
    {
        cooldownTimers[abilityID] = serverTimeStamp;
    }


}
