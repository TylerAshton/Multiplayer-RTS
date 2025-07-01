using System.Collections.Generic;
using UnityEngine;

public static class ProjectileStatsRegistry
{
    private static Dictionary<string, ProjectileStats> projectileStats = new Dictionary<string, ProjectileStats>();

    public static IReadOnlyDictionary<string, ProjectileStats> ProjectileStats => projectileStats;

    /// <summary>
    /// Adds the ProjectileStats into the Abilities dictionary
    /// </summary>
    /// <param name="_statID"></param>
    /// <param name="_stat"></param>
    public static void Register(string _statID, ProjectileStats _stat)
    {
        if (projectileStats.ContainsKey(_statID))
        {
            Debug.LogError($"Attempted to register an ability ({_statID}) that is alrady registered");
            return;
        }
        if (_statID == null || _statID == string.Empty)
        {
            Debug.LogError($"Ability ID of {_stat.name} | {_statID} is null or empty.");
            return;
        }

        projectileStats.Add(_statID, _stat);
    }

    /// <summary>
    /// Gets the ability from the Abilities dictionary with the parsed ID
    /// </summary>
    /// <param name="_statID"></param>
    /// <returns></returns>
    public static ProjectileStats GetProjectileStat(string _statID)
    {
        ProjectileStats output = projectileStats.TryGetValue(_statID, out ProjectileStats stat) ? stat : null;

        if (output == null)
        {
            Debug.LogError($"ProjectileStat ID {_statID} does not exist in the registry.");
        }

        return output;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoRegisterAll()
    {
        projectileStats.Clear();

        var all = Resources.LoadAll<ProjectileStats>($"{nameof(projectileStats)}");
        foreach (var a in all)
        {
            Register(a.ID, a);
        }
    }
}
