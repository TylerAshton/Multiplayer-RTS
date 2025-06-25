using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    [SerializeField] float duration = 5f;
    [SerializeField] private List<StatModifyer> statModifyers = new List<StatModifyer>();
    public List<StatModifyer> StatModifyers => new List<StatModifyer>(statModifyers);
    public float Duration => duration;
    public abstract void OnStart(EffectManager _effectManager);

    public abstract void OnUpdate(EffectManager _effectManager);

    public abstract void OnEnd(EffectManager _effectManager);
}
