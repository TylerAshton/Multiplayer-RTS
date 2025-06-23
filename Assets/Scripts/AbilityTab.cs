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
    [SerializeField] private List<Ability> abilities = new List<Ability>();
    public List<Ability> Abilities => new List<Ability>(abilities);



    public void SetAbility(int _abilityIndex, Ability _ability)
    {
        if (_abilityIndex < 0 || _abilityIndex >= abilities.Count)
        {
            Debug.LogError($"Invalid ability index: {_abilityIndex}. Must be between 0 and {abilities.Count - 1}.");
            return;
        }

        abilities[_abilityIndex] = _ability;
    }

    public void OverrideList(List<Ability> _abilities)
    {
        if (_abilities == null)
        {
            Debug.LogError("Cannot override with a null list of abilities.");
            return;
        }

        abilities = new List<Ability>(_abilities);
    }

    public void AddAbility(Ability _ability)
    {
        abilities.Add(_ability);
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
            abilities = new List<Ability>(this.abilities)
        };
        return clone;
    }
}