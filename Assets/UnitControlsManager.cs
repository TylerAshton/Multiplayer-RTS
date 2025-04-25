using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class UnitControlsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> abilityCells = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetAbilityGrid();
    }

    /// <summary>
    /// Disables and hides all ability cells in the grid
    /// </summary>
    public void ResetAbilityGrid()
    {
        foreach (GameObject _cell in abilityCells)
        {
            Image cellImage = _cell.GetComponent<Image>();
            Button cellButton = _cell.GetComponent<Button>();

            cellImage.enabled = false;
            cellButton.interactable = false;
        }
    }

    /// <summary>
    /// Shows
    /// </summary>
    /// <param name="_ability"></param>
    /// <param name="_cell"></param>
    private void SetAbilityCell(Ability _ability, GameObject _cell, List<Unit> _selectedUnits)
    {
        Image cellImage = _cell.GetComponent<Image>();
        Button cellButton = _cell.GetComponent<Button>();

        cellImage.enabled = true;
        cellButton.interactable = true;

        cellImage.sprite = _ability.Icon;

        // Add Event bindings to button pressed
        cellButton.onClick.RemoveAllListeners();

        cellButton.onClick.AddListener(() =>
        {
            foreach (Unit _unit in _selectedUnits)
            {
                int unitIndex = _unit.AbilityManager.Abilities.IndexOf(_ability);
                if (unitIndex >= 0)
                {
                    _unit.AbilityManager.TryCastAbility(unitIndex);
                }
            }
        });


    }

    public void UpdateGridWithUnitSelection(List<Unit> _selectedUnits)
    {
        ResetAbilityGrid();
        List<Ability> commonAbilities = GetCommonAbilities(_selectedUnits);

        int cellIndex = 0;

        for (int i = 0; i < commonAbilities.Count; i++)
        {
            SetAbilityCell(commonAbilities[i], abilityCells[cellIndex], _selectedUnits);
            cellIndex++;
        }
    }

    /// <summary>
    /// Returns a list of abilities that are common across the parsed list of units
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    private List<Ability> GetCommonAbilities(List<Unit> _units)
    {
        if (_units == null || _units.Count == 0)
        {
            Debug.LogError("Cannot get common abilities from an empty or null unit list.");
            return new List<Ability>();
        }

        List<Ability> commonAbilities = _units[0].AbilityManager.Abilities;

        if (_units.Count == 1) // If there's only 1 unit no need to scan for common
        {
            return commonAbilities;
        }

        // Eliminate uncommon abilities in all other units to our list of commonAbilities
        for (int i = 1;  i < _units.Count; i++)
        {
            Unit unit = _units[i];

            // Iterate backwards to correctly remove unfound abilities while looping
            for (int x = commonAbilities.Count - 1; x >= 0; x--)
            {
                if (!unit.AbilityManager.Abilities.Contains(commonAbilities[x]))
                {
                    commonAbilities.RemoveAt(x);
                }
            }
        }

        return commonAbilities;
    }
}
