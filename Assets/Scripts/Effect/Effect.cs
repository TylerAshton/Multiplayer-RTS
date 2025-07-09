using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect
{
    [SerializeField] float duration;
    [SerializeField] private List<StatModifyer> statModifyers;
    public List<StatModifyer> StatModifyers => new List<StatModifyer>(statModifyers);
    public float Duration => duration;

    public Effect() // Inspector friendly constructor
    {
        
    } 

    public Effect(float _duration, List<StatModifyer> _modifiers) // Manual constructor for runtime.
    {
        duration = _duration;
        statModifyers = _modifiers;
    }


}
