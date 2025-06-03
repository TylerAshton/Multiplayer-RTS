using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        abilities[_abilityIndex] = _ability;
    }

    public void OverrideList(List<Ability> _abilities)
    {
        abilities = new List<Ability>(_abilities);
    }

    public void AddAbility(Ability _ability)
    {
        abilities.Add(_ability);
    }

    /*    public List<Ability> GetAbilities()
        {
            return abilities.Select(tab => tab.Clone()).ToList();
        }*/

    /// <summary>
    /// Returns a read-only variant of this AbilityTab.
    /// </summary>
    /// <returns></returns>
    public AbilityTab Clone()
    {
        AbilityTab clone = new AbilityTab
        {
            tabName = this.tabName,
            abilities = new List<Ability>(this.Abilities)
        };
        return clone;
    }
}