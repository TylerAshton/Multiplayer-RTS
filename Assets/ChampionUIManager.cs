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
    private void SetAbilityCell(Ability _ability, GameObject _cell, ChampionAbilityManager _abilityManager)
    {
        if (_ability.Icon == null)
        {
            Debug.LogError("Tried to display ability in UI which has no icon");
        }
        Image cellImage = _cell.GetComponent<Image>();
        Button cellButton = _cell.GetComponent<Button>();

        cellImage.enabled = true;
        cellButton.interactable = true;

        cellImage.sprite = _ability.Icon;

        // Add Event bindings to button pressed
        cellButton.onClick.RemoveAllListeners();

        cellButton.onClick.AddListener(() =>
        {
            int unitIndex = _abilityManager.Abilities.IndexOf(_ability);
            if (unitIndex >= 0) // TODO: wait why is this line here?
            {
                _abilityManager.TryCastAbility(unitIndex);
            }
        });
    }

    public void UpdateGridWithChampionAbilities(ChampionAbilityManager _abilityManager)
    {
        ResetAbilityGrid();

        List<Ability> abilities = _abilityManager.Abilities;

        int cellIndex = 0;

        for (int i = 0; i < abilities.Count; i++)
        {
            SetAbilityCell(abilities[i], abilityCells[cellIndex], _abilityManager);
            cellIndex++;
        }
    }
}
