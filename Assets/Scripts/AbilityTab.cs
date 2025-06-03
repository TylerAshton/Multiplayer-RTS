using NUnit.Framework;
using System.Collections.Generic;
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

    public void AddAbility(Ability _ability)
    {
        abilities.Add(_ability);
    }
}