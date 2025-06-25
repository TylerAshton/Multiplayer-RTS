using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    AttackSpeed,
    MoveSpeed
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

    private Dictionary<StatType, float> baseStats = new();
    private Dictionary<StatType , float> currentStats = new();
    private List<StatModifyer> statModifyers = new List<StatModifyer>();

    public IReadOnlyDictionary<StatType, float> CurrentStats => currentStats;


    private void OnValidate()
    {
        baseStats.Clear();
        foreach (var entry in baseStatsList)
        {
            baseStats[entry.statType] = entry.value;
        }
    }

    public void AddStatModifyer(StatModifyer _statModifyer)
    {
        if (_statModifyer == null)
        {
            Debug.LogError($"{nameof(_statModifyer)} cannot be null in {nameof(AddStatModifyer)}!");
            return;
        }

        statModifyers.Add(_statModifyer);

        RecalculateCurrentStats();
    }

    public void RemoveStatModifyer(StatModifyer _statModifyer)
    {
        if (_statModifyer == null)
        {
            Debug.LogError($"{nameof(_statModifyer)} cannot be null in {nameof(RemoveStatModifyer)}!");
            return;
        }

        if (!statModifyers.Contains(_statModifyer))
        {
            Debug.LogError($"{nameof(_statModifyer)} : {_statModifyer.name} doesn't exist in {nameof(statModifyers)} in gameobject {gameObject.name}");
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
        
        foreach (var stat in baseStats)
        {
            currentStats[stat.Key] = stat.Value;
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
        }
    }
    
}
