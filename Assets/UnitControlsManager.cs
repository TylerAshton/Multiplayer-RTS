using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitControlsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> abilityCells = new List<GameObject>();
    [SerializeField] private Ability testAbility;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetAbilityGrid();
        SetAbilityCell(testAbility, abilityCells[0]);
    }

    /// <summary>
    /// Disables and hides all ability cells in the grid
    /// </summary>
    private void ResetAbilityGrid()
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
    private void SetAbilityCell(Ability _ability, GameObject _cell)
    {
        Image cellImage = _cell.GetComponent<Image>();
        Button cellButton = _cell.GetComponent<Button>();

        cellImage.enabled = true;
        cellButton.interactable = true;

        cellImage.sprite = _ability.Icon;
    }
}
