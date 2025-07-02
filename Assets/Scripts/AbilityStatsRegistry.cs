using System.Collections.Generic;
using UnityEngine;

public static class AbilityStatsRegistry
{
    private static Dictionary<string, BaseAbilityStat> abilityStats = new Dictionary<string, BaseAbilityStat>();

    public static IReadOnlyDictionary<string, BaseAbilityStat> AbilityStats => abilityStats;

    /// <summary>
    /// Adds the BaseAbilityStat into the Abilities dictionary
    /// </summary>
    /// <param name="_statID"></param>
    /// <param name="_stat"></param>
    public static void Register(string _statID, BaseAbilityStat _stat)
    {
        if (abilityStats.ContainsKey(_statID))
        {
            Debug.LogError($"Attempted to register an ability ({_statID}) that is alrady registered");
            return;
        }
        if (_statID == null || _statID == string.Empty)
        {
            Debug.LogError($"Ability ID of {_stat.name} | {_statID} is null or empty.");
            return;
        }

        abilityStats.Add(_statID, _stat);
    }

    /// <summary>
    /// Gets the (T)abilityStat from the AbilityStat dictionary with the parsed ID where T is the derived type of the AbilityStat
    /// </summary>
    /// <param name="_statID"></param>
    /// <returns></returns>
    public static T GetProjectileStat<T>(string _statID) where T : BaseAbilityStat
    {
        BaseAbilityStat output = abilityStats.TryGetValue(_statID, out BaseAbilityStat stat) ? stat : null;

        if (output == null)
        {
            Debug.LogError($"{nameof(T)} ID {_statID} does not exist in the registry.");
        }
        if (output is T derivedForm)
        {
            return derivedForm;
        }
        else
        {
            Debug.LogError($"{output.name} - {_statID} is not a {nameof(T)}");
            return null;
        }
    }

    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoRegisterAll()
    {
        abilityStats.Clear();

        var all = Resources.LoadAll<BaseAbilityStat>($"AbilityStats");
        foreach (var a in all)
        {
            Register(a.ID, a);
        }
    }
}
