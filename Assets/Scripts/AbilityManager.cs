using System;
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
    public IReadOnlyDictionary<string, float> CooldownTimers => cooldownTimers;

    private float AttackSpeed = 1;

    private NetworkObject networkObject;

    private ulong ownerClientId = 999999;

    [SerializeField] private bool hasUtility = false;
    public bool HasUtility => hasUtility;
    private bool isUtilityEnabled = false;
    public bool IsUtilityEnabled => isUtilityEnabled;

    public event Action OnAbilitiesChanged;


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

        if (_ability == null)
        {
            Debug.LogError("Cannot add a null ability");
            return;
        }

        if (_tabIndex < 0 || _tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid tab index: {_tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }

        SetAbilityRpc(_index, _ability.ID, _tabIndex);
    }

    [Rpc(SendTo.Everyone)]
    private void SetAbilityRpc(int _abilityIndex, string _abilityID, int _tabIndex)
    {
        Ability ability = Registry<Ability>.GetItem(_abilityID);

        if (ability == null)
        {
            Debug.LogError("abilityID parsed doesn't match an ability!");
            return;
        }

        if (_tabIndex < 0 || _tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid tab index: {_tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }


        abilityTabs[_tabIndex].SetAbility(_abilityIndex, ability);
        OnAbilitiesChanged?.Invoke();
    }

    public virtual void AddAbility(Ability _ability, int _tabIndex)
    {
        if (_ability == null)
        {
            Debug.LogError("Cannot add a null ability");
            return;
        }

        if (_tabIndex < 0 || _tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid tab index: { _tabIndex }. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }

        if (!IsServer)
        {
            Debug.LogError("Client attempted to add an ability");
            return;
        }

        int predIndex = TryFindPrediscessorIndex(_ability, _tabIndex);

        if (predIndex != -1)
        {
            SetAbility(predIndex, _ability, _tabIndex);
            return;
        }

        AddAbilityRpc(_ability.ID, _tabIndex);
    }

    private int TryFindPrediscessorIndex(Ability _ability, int _tabIndex)
    {
        if (_ability == null || _tabIndex < 0 || _tabIndex >= abilityTabs.Count)
        {
            Debug.LogError("Invalid ability or tab index");
            return -1;
        }
        AbilityTab tab = abilityTabs[_tabIndex];
        for (int i = 0; i < tab.Abilities.Count; i++)
        {
            if (tab.Abilities[i].Successor == _ability)
            {
                return i;
            }
        }
        return -1;
    }

    [Rpc(SendTo.Everyone)]
    private void AddAbilityRpc(string _abilityID, int _tabIndex)
    {
        Ability ability = Registry<Ability>.GetItem(_abilityID);

        if (ability == null)
        {
            Debug.LogError("abilityID parsed doesn't match an ability!");
            return;
        }

        if (_tabIndex < 0 || _tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid ability index: {_tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }

        abilityTabs[_tabIndex].AddAbility(ability);
        OnAbilitiesChanged?.Invoke();
    }

    public void RemoveAbility(Ability _ability, int tabIndex = -1)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to remove an ability");
            return;
        }
        if (_ability == null)
        {
            Debug.LogError("Cannot remove a null ability");
            return;
        }

        tabIndex = tabIndex == -1 ? FindAbilityTabIndex(_ability) : tabIndex;

        if (tabIndex == -1)
        {
            Debug.LogError("Ability not found in any tab.");
            return;
        }

        if (tabIndex < 0 || tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid tab index: {tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }
        RemoveAbilityRpc(_ability.ID, tabIndex);
    }

    [Rpc(SendTo.Everyone)]
    private void RemoveAbilityRpc(string _abilityID, int tabIndex)
    {
        Ability ability = Registry<Ability>.GetItem(_abilityID);

        if (ability == null)
        {
            Debug.LogError("abilityID parsed doesn't match an ability!");
            return;
        }

        if (tabIndex < 0 || tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid tab index: {tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return;
        }
        AbilityTab selectedTab = abilityTabs[tabIndex];
        Ability abilityToRemove = selectedTab.Abilities.FirstOrDefault(a => a == ability);

        if (abilityToRemove == null)
        {
            Debug.LogError($"Ability with ID {_abilityID} not found in tab {tabIndex}.");
            return;
        }

        selectedTab.RemoveAbility(abilityToRemove);
        OnAbilitiesChanged?.Invoke();
    }

    public bool CheckAbility(Ability _ability, int tabIndex = -1)
    {
        if (_ability == null)
        {
            Debug.LogError("Cannot check a null ability");
            return false;
        }

        tabIndex = tabIndex == -1 ? FindAbilityTabIndex(_ability) : tabIndex;

        if (tabIndex == -1)
        {
            Debug.LogError("Ability not found in any tab.");
            return false;
        }

        if (tabIndex < 0 || tabIndex >= abilityTabs.Count)
        {
            Debug.LogError($"Invalid ability index: {tabIndex}. Must be between 0 and {abilityTabs.Count - 1}.");
            return false;
        }

        return abilityTabs[tabIndex].Abilities.Contains(_ability);

    }

    /// <summary>
    /// Returns the index of the tab that contains the specified ability.
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private int FindAbilityTabIndex(Ability _ability)
    {
        for (int i = 0; i < abilityTabs.Count; i++)
        {
            if (abilityTabs[i].Abilities.Contains(_ability))
            {
                return i;
            }
        }
        return -1;
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
    public void TryCastAbility(int _abilityIndex, int tabIndex)
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

        SetCooldownRpc(_ability.ID);
    }

    /// <summary>
    /// Updates the clients with the cooldown of said ability
    /// </summary>
    /// <param name="_abilityID"></param>
    /// <param name="_serverTimeStamp"></param>
    [Rpc(SendTo.Everyone)]
    private void SetCooldownRpc(string _abilityID)
    {
        cooldownTimers[_abilityID] = Time.time;
    }

    public void ToggleUtility()
    {
        isUtilityEnabled = !isUtilityEnabled;
    }
}
