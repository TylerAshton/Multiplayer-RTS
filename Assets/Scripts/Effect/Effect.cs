using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Effect
{
    [SerializeField] float duration;
    [SerializeField] private List<StatModifyer> statModifyers;
    public List<StatModifyer> StatModifyers => new List<StatModifyer>(statModifyers);
    public float Duration => duration;

    public Effect(float _duration, List<StatModifyer> _modifiers)
    {
        duration = _duration;
        statModifyers = _modifiers;
    }

}
