using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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

    private Coroutine lockCastingCoroutine = null;

    /*    [SerializeField] protected List<Ability> abilities;*/


    [SerializeField] private List<AbilityTab> abilityTabs = new List<AbilityTab>();
    public List<AbilityTab> AbilityTabs => GetAbilityTabs(); // This prevents the list CONTENTS from being fucked with

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

    private List<AbilityTab> GetAbilityTabs()
    {
        return abilityTabs.Select(tab => tab.Clone()).ToList();
    }

    public virtual void SetAbility(int _index, Ability _ability, int _tabIndex)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to set an ability");
            return;
        }
        SetAbilityRpc(_index, _ability.ID, _tabIndex);
    }

    [Rpc(SendTo.Everyone)]
    private void SetAbilityRpc(int _abilityIndex, string _abilityID, int tabIndex)
    {
        abilityTabs[tabIndex].SetAbility(_abilityIndex, Registry<Ability>.GetItem(_abilityID));
    }

    public virtual void AddAbility(Ability _ability, int _tabIndex)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to add an ability");
            return;
        }
        AddAbilityRpc(_ability.ID, _tabIndex);

        // TODO: Harrison please update abilityGrid
    }

    [Rpc(SendTo.Everyone)]
    private void AddAbilityRpc(string _abilityID, int _tabIndex)
    {
        abilityTabs[_tabIndex].AddAbility(Registry<Ability>.GetItem(_abilityID));
    }

    public bool CheckAbility(Ability _ability, int tabIndex = 0)
    {
        if (tabIndex < 0 || tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid ability index: {tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return false;
        }

        return abilityTabs[tabIndex].Abilities.Contains(_ability);

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
    public void TryCastAbility(int _abilityIndex, int tabIndex = 0)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to cast an ability");
            return;
        }

        if (tabIndex < 0)
        {
            Debug.LogError("Tab index cannot be negative: " + tabIndex);
            return;
        }

        if (abilityTabs.Count <= tabIndex || abilityTabs[tabIndex] == null)
        {
            Debug.LogError("Tab index out of range or doesn't exist: " +tabIndex);
            return;
        }

        AbilityTab selectedTab = abilityTabs[tabIndex];

        if (_abilityIndex < 0)
        {
            Debug.LogError("Ability index cannot be negative: " + _abilityIndex);
            return;
        }

        if (selectedTab.Abilities.Count <= _abilityIndex || selectedTab.Abilities[_abilityIndex] == null)
        {
            Debug.LogError($"Ability index out of range: {_abilityIndex} in gameobject: {gameObject.name}!");
            return;
        }

        Ability selectedAbility = selectedTab.Abilities[_abilityIndex];


        if (!CanCastAbility(selectedAbility))
        {
            Debug.LogWarning("Cannot cast ability due to checks failing");
            return;
        }

        currentAbility = selectedAbility;
        StartCooldown(currentAbility);
        PointManager.Instance.RemovePoints(ownerClientId, currentAbility.AbilityCost);
        currentAbility.Activate(abilityUser);
        if (lockCastingCoroutine != null)
        {
            StopCoroutine(lockCastingCoroutine);
        }
        lockCastingCoroutine = StartCoroutine(LockCastingUntil(currentAbility.CastTime));
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
        if (cooldownTimers.TryGetValue(_ability.ID, out float lastUsedTime))
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

        cooldownTimers[_ability.ID] = Time.time;
        SetCooldownRpc(_ability.ID, Time.time);
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
