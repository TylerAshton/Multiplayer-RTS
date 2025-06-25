using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private List<Effect> activeEffects = new List<Effect>();
    private AbilityManager abilityManager;
    public AbilityManager AbilityManager => abilityManager;

    private Health health;
    public Health Health => health;
    private StatManager statManager;
    public StatManager StatManager => statManager;

    private void Awake()
    {
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError($"{nameof(AbilityManager)} is required for {nameof(EffectManager)} in gameObject {gameObject.name}");
            return;
        }
        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError($"{nameof(Health)} is required for {nameof(EffectManager)} in gameObject {gameObject.name}");
            return;
        }
        if (!TryGetComponent<StatManager>(out statManager))
        {
            Debug.LogError($"{nameof(StatManager)} is required for {nameof(EffectManager)} in gameObject {gameObject.name}");
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
/*        foreach(Effect _effect in activeEffects)
        {
            _effect.OnUpdate(this);
        }*/
    }

    public void AddEffect(Effect _effect)
    {
        activeEffects.Add(_effect);
        foreach(StatModifyer _statModifyer in _effect.StatModifyers)
        {
            statManager.AddStatModifyer(_statModifyer);
        }

        //_effect.OnStart(this);
        StartCoroutine(EffectTimer(_effect, _effect.Duration));

    }

    public void EndEffect(Effect _effect)
    {
        foreach (StatModifyer _statModifyer in _effect.StatModifyers)
        {
            statManager.RemoveStatModifyer(_statModifyer);
        }
        //_effect.OnEnd(this);
        activeEffects.Remove(_effect);
    }

    private IEnumerator EffectTimer(Effect _effect, float _timer)
    {
        yield return new WaitForSeconds(_timer);
        EndEffect(_effect);
    }
}
