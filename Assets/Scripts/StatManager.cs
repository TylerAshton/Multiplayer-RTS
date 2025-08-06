using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    AttackSpeed,
    MoveSpeed,
    HealthRegeneration,
    RotationSpeed
}

/// <summary>
/// Fucking stat entry because dicitonaries suck dick
/// </summary>
[System.Serializable]
public struct StatEntry
{
    public StatType statType;
    public float value;
}
public class StatManager : MonoBehaviour
{
    [SerializeField]
    private List<StatEntry> baseStatsList = new();

    private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
    {   // Default values
        { StatType.AttackSpeed, 1f },
        { StatType.MoveSpeed, 5f },
        { StatType.RotationSpeed, 1000f },
        { StatType.HealthRegeneration, 0f }
    };

    private Dictionary<StatType , float> currentStats = new();
    private List<StatModifyer> statModifyers = new List<StatModifyer>();
    private Animator animator;

    public IReadOnlyDictionary<StatType, float> CurrentStats => currentStats;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError($"{nameof(Animator)} is required for {GetType().Name} on gameobject: {gameObject.name}");
        }


    }

    private void Start()
    {
        foreach (var entry in baseStatsList)
        {
            baseStats[entry.statType] = entry.value;
        }

        RecalculateCurrentStats();
    }

    public void SetAttackSpeed(float _attackSpeed)
    {
        animator.SetFloat("AttackSpeed", _attackSpeed);
    }

    public void AddStatModifyer(StatModifyer _statModifyer)
    {
/*        if (_statModifyer == null)
        {
            Debug.LogError($"{nameof(_statModifyer)} cannot be null in {nameof(AddStatModifyer)}!");
            return;
        }*/

        statModifyers.Add(_statModifyer);

        RecalculateCurrentStats();
    }

    public void RemoveStatModifyer(StatModifyer _statModifyer)
    {
/*        if (_statModifyer == null)
        {
            Debug.LogError($"{nameof(_statModifyer)} cannot be null in {nameof(RemoveStatModifyer)}!");
            return;
        }*/

        if (!statModifyers.Contains(_statModifyer))
        {
            Debug.LogError($"{nameof(_statModifyer)} doesn't exist in {nameof(statModifyers)} in gameobject {gameObject.name}");
            return;
        }

        statModifyers.Remove(_statModifyer);

        RecalculateCurrentStats();
    }

    /// <summary>
    /// When a statModifyer is added or removed this is called ot calculate the currentStats
    /// </summary>
    private void RecalculateCurrentStats()
    {
        // Reset and copy base stats
        currentStats.Clear();
        
        foreach (var _stat in baseStats)
        {
            currentStats[_stat.Key] = _stat.Value;

            if (_stat.Key == StatType.AttackSpeed) // TODO: We'll have to optimise this later
            {
                SetAttackSpeed(_stat.Value);
            }
        }

        foreach (var _modifier in statModifyers)
        {
            if(!currentStats.ContainsKey(_modifier.StatType))
            {
                Debug.LogError($"{nameof(currentStats)} is missing a stat: {nameof(_modifier.StatType)} within gameobject {gameObject.name}!");
                return;
            }

            // Before
            float newStat = currentStats[_modifier.StatType];
            newStat += _modifier.Value;

            // Save
            currentStats[_modifier.StatType] = newStat;

            if (_modifier.StatType == StatType.AttackSpeed) // TODO: We'll have to optimise this later
            {
                SetAttackSpeed(newStat);
            }
        }
    }
    
}
