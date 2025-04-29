using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class ChampionUIManager : MonoBehaviour // TODO this is done the same in rts, make a base class please
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
}
