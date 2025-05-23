using System.Collections.Generic;
using UnityEngine;

public static class AbilityRegistry
{
    private static Dictionary<string, Ability> abilities = new Dictionary<string, Ability>();

    public static IReadOnlyDictionary<string, Ability> Abilities => abilities; 

    /// <summary>
    /// Adds the ability into the Abilities dictionary
    /// </summary>
    /// <param name="_abilityID"></param>
    /// <param name="_ability"></param>
    public static void Register(string _abilityID, Ability _ability)
    {
        if (abilities.ContainsKey(_abilityID))
        {
            Debug.LogError($"Attempted to register an ability ({_abilityID}) that is alrady registered");
            return;
        }
        if (_abilityID == null || _abilityID == string.Empty)
        {
            Debug.LogError($"Ability ID {_abilityID} is null or empty.");
            return;
        }

        abilities.Add(_abilityID, _ability);
    }

    /// <summary>
    /// Gets the ability from the Abilities dictionary with the parsed ID
    /// </summary>
    /// <param name="_abilityID"></param>
    /// <returns></returns>
    public static Ability GetAbility(string _abilityID)
    {
        Ability output = abilities.TryGetValue(_abilityID, out Ability ability) ? ability : null;

        if (output == null)
        {
            Debug.LogError($"Ability ID {_abilityID} does not exist in the registry.");
        }

        return output;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoRegisterAll()
    {
        abilities.Clear();

        var all = Resources.LoadAll<Ability>("Abilities");
        foreach (var a in all)
        {
            Register(a.AbilityID, a);
        }
    }

}
