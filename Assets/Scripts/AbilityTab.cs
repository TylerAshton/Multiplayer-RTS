using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;

/// <summary>
/// This is a wrapper class for the ability tabs units can use to organize their abilities.
/// </summary>
[System.Serializable]
public class AbilityTab
{
    public string tabName = "newTab";
    [SerializeField] private List<AbilityReference> abilityReferences = new List<AbilityReference>();
    public List<AbilityReference> AbilityReferences => new List<AbilityReference>(abilityReferences);

    // TODO: This is being called a lot, cache the result maybe?
    public List<Ability> Abilities => abilityReferences.Select(ar => ar.ability).ToList(); // Fancy LINQ to get a list of abilities from the references


    public void SetAbility(int _abilityIndex, Ability _ability)
    {
        if (_abilityIndex < 0 || _abilityIndex >= abilityReferences.Count)
        {
            Debug.LogError($"Invalid ability index: {_abilityIndex}. Must be between 0 and {abilityReferences.Count - 1}.");
            return;
        }

        abilityReferences[_abilityIndex].ability = _ability;
    }

    public void OverrideList(List<Ability> _abilities)
    {
        if (_abilities == null || _abilities.Count == 0)
        {
            Debug.LogError("Cannot override with an empty or null list of abilities.");
            return;
        }
        List<AbilityReference> refs = _abilities.Select(a => new AbilityReference { ability = a }).ToList();


        abilityReferences = new List<AbilityReference>(refs);
    }

    public void AddAbility(Ability _ability)
    {
        AbilityReference abilityReference = new AbilityReference { ability = _ability };
        abilityReferences.Add(abilityReference);
    }

    /// <summary>
    /// Returns a read-only variant of this AbilityTab.
    /// </summary>
    /// <returns></returns>
    public AbilityTab Clone()
    {
        AbilityTab clone = new AbilityTab
        {
            tabName = this.tabName,
            abilityReferences = new List<AbilityReference>(this.AbilityReferences)
        };
        return clone;
    }
}