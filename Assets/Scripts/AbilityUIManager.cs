using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> abilityCells = new List<GameObject>();
    [SerializeField] private Sprite forwardSprite;
    [SerializeField] private Sprite backSprite;
    private int pageIndex = 0;
    private int tabIndex = 0;
    private List<Ability> commonAbilities;
    private List<AbilityManager> abilityManagers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ResetAbilityGrid();
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
    private void SetAbilityCell(Ability _ability, GameObject _cell, List<AbilityManager> _abilityManagers)
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
            foreach (AbilityManager _abilityManager in _abilityManagers)
            {
                int abilityIndex = _abilityManager.AbilityTabs[tabIndex].abilities.IndexOf(_ability);
                if (abilityIndex >= 0)
                {
                    _abilityManager.TryCastAbility(abilityIndex);
                }
            }
        });
    }

    private void SetPageCell(GameObject _cell, int _pageIndex)
    {
        Image cellImage = _cell.GetComponent<Image>();
        Button cellButton = _cell.GetComponent<Button>();

        cellImage.enabled = true;
        cellButton.interactable = true;

        cellImage.sprite = (_pageIndex > pageIndex) ? forwardSprite: backSprite;

        cellButton.onClick.RemoveAllListeners();

        cellButton.onClick.AddListener(() =>
        {
            this.SetPage(_pageIndex);
        });
    }

    public void SetPage(int _newPageIndex)
    {
        pageIndex = _newPageIndex;

        RefreshGrid();
    }

    public void UpdateGridWithUnitSelection(List<SelectableObject> _selectedUnits)
    {
        pageIndex = 0;
        commonAbilities = GetCommonAbilities(_selectedUnits);
        abilityManagers = _selectedUnits.Select(i => i.AbilityManager).ToList();

        RefreshGrid();
    }

    /// <summary>
    /// Updates the ability grid with the abilities of the passed in ability manager
    /// </summary>
    /// <param name="_abilityManager"></param>
    public void UpdateGridWithAbilityManager(AbilityManager _abilityManager)
    {
        pageIndex = 0; // TODO: Unsure about setting it to zero straight up?
        commonAbilities = _abilityManager.AbilityTabs[tabIndex].abilities;
        abilityManagers = new List<AbilityManager>() { _abilityManager };

        RefreshGrid();
    }

    private void RefreshGrid()
    {
        ResetAbilityGrid();

        if (pageIndex == 2)
        {
            Debug.Log("f");
        }

        // Calculate how many abilities have already been shown on previous pages.
        // pageNumber is 0, then we have no skipped abilities.
        // pageNumber is 1, then we have 3 + 0 = 3 skipped abilities, as 1 cell is used for nav buttons.
        // pageNumber is 2, then we have 3 + 2 = 5 skipped abilities, as 3 cell is used for nav button.
        int skippedAbilities = 0;

        if (pageIndex > 0)
        {
            skippedAbilities = 3 + (pageIndex - 1) * (abilityCells.Count - 2);
        }

        Queue<Ability> abilitiesInPage = 
        new Queue<Ability>(commonAbilities.GetRange(skippedAbilities, commonAbilities.Count - skippedAbilities));

        int abilitiesRemaining = abilitiesInPage.Count;

        for (int cellIndex = 0; cellIndex < Mathf.Min(abilityCells.Count, abilitiesRemaining); cellIndex++)
        {
            switch (cellIndex)
            {
                case 0:
                    if (pageIndex > 0)
                    {
                        SetPageCell(abilityCells[cellIndex], pageIndex - 1);
                    }
                    else
                    {
                        SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                    }
                    break;
                case 1:
                    SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                    break;
                case 2:
                    SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                    break;
                case 3:
                    if (abilitiesInPage.Count > 1)
                    {
                        SetPageCell(abilityCells[cellIndex], pageIndex + 1);
                    }
                    else
                    {
                        SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Returns a list of abilities that are common across the parsed list of units
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    private List<Ability> GetCommonAbilities(List<SelectableObject> _units)
    {
        if (_units == null || _units.Count == 0)
        {
            Debug.LogError("Cannot get common abilities from an empty or null unit list.");
            return new List<Ability>();
        }

        List<Ability> commonAbilities = _units[0].AbilityManager.AbilityTabs[tabIndex].abilities;

        if (_units.Count == 1) // If there's only 1 unit no need to scan for common
        {
            return commonAbilities;
        }

        // Eliminate uncommon abilities in all other units to our list of commonAbilities
        for (int i = 1; i < _units.Count; i++)
        {
            SelectableObject unit = _units[i];

            // Iterate backwards to correctly remove unfound abilities while looping
            for (int x = commonAbilities.Count - 1; x >= 0; x--)
            {
                if (!unit.AbilityManager.AbilityTabs[tabIndex].abilities.Contains(commonAbilities[x]))
                {
                    commonAbilities.RemoveAt(x);
                }
            }
        }

        return commonAbilities;
    }
}
