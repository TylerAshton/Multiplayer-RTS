using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

/// <summary>
/// This manages an inspector friendly dictionary of all the ability position transforms that
/// an AbilityCaster requires
/// </summary>
public class AbilityPositionManager : MonoBehaviour
{
    [SerializeField]
    private List<AbilityPositionEntry> abilityPositionsEntries;

    private Dictionary<AbilityPosition, Transform> abilityPositions; // Define in awake with struct

    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositions;

    private void Awake()
    {
        MapAbilityPositions();
    }


    /// <summary>
    /// Using the struct list of abilityPositionEntires populate and map the dictionary with the list
    /// </summary>
    private void MapAbilityPositions()
    {
        abilityPositions = new Dictionary<AbilityPosition, Transform>();

        foreach (AbilityPositionEntry entry in abilityPositionsEntries)
        {
            abilityPositions[entry.key] = entry.value;
        }
    }
}
