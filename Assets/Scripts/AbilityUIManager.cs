using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    [SerializeField] private List<AbilityCell> abilityCells = new List<AbilityCell>();
    [SerializeField] private List<GameObject> abilityTabButtons = new List<GameObject>();
    [SerializeField] private Sprite forwardSprite;
    [SerializeField] private Sprite backSprite;
    private int pageIndex = 0;
    private int tabIndex = 0;
    private List<AbilityTab> commonAbilityTabs;
    private List<Ability> commonAbilities
    {   
        get  
        {
            if (commonAbilityTabs == null || commonAbilityTabs.Count == 0)
            {
                return new List<Ability>();
            }
            return commonAbilityTabs[tabIndex].Abilities;
        }
    }
    private List<AbilityManager> abilityManagers = new List<AbilityManager>();
    private bool isChampionUI = false;

    internal void Init(bool _isChampionUI)
    {
        isChampionUI = _isChampionUI;
    }

    private void Update()
    {
        ShowCooldowns();
    }

    /// <summary>
    /// Updates the UI to show the cooldowns of the common abilities. Or at least the 
    /// </summary>
    private void ShowCooldowns() // TODO: Self contain abilityCells if we have time
    {
        if (abilityManagers.Count <= 0)
        {
            return;
        }

        if (commonAbilities.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < commonAbilities.Count; i++)
        {
            Slider slider = abilityCells[i].Slider;
            Ability ability = commonAbilities[i];

            float cooldownStartTime = GetLongestCooldown(ability);

            if (cooldownStartTime == 0)
            {
                slider.value = 0;
                return; // No cooldown needed to calculate
            }

            // Calculate remaining time until end of cooldown
            slider.maxValue = ability.Cooldown;

            float cooldownEndTime = ability.Cooldown + cooldownStartTime;
            float timePassed = Time.time - cooldownStartTime;
            float timeRemaining = ability.Cooldown - timePassed;

            slider.value = timeRemaining;
        }
    }

    /// <summary>
    /// Returns the longest cooldown value among the abilityManagers with the parsed Ability
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private float GetLongestCooldown(Ability _ability)
    {
        if (!commonAbilities.Contains(_ability))
        {
            Debug.LogError($"{_ability.name} is not a common abiltiy!");
            return 0;
        }

        float longestCooldown = 0f;

        foreach (AbilityManager abilityManager in abilityManagers)
        {
            float cooldownStartTime = abilityManager.CooldownTimers.TryGetValue(_ability.ID, out float value) ? value : 0f;

            if (value > longestCooldown)
            {
                longestCooldown = value;
            }
        }

        return longestCooldown;
    }

    /// <summary>
    /// Disables and hides all ability cells in the grid
    /// </summary>
    public void ResetAbilityGrid()
    {
        foreach (AbilityCell _cell in abilityCells)
        {
            _cell.Image.enabled = false;
            _cell.Button.interactable = false;
            _cell.Slider.value = 0;
        }
    }

    /// <summary>
    /// Shows
    /// </summary>
    /// <param name="_ability"></param>
    /// <param name="_cell"></param>
    private void SetAbilityCell(Ability _ability, AbilityCell _cell, List<AbilityManager> _abilityManagers)
    {
        if (_ability == null)
        {
            Debug.LogError($"{nameof(_ability)} was null in {gameObject.name}!");
            return;
        }

        if (_cell == null)
        {
            Debug.LogError($"{nameof(_cell)} was null in {gameObject.name}");
            return;
        }
        if (_abilityManagers == null || _abilityManagers.Count == 0)
        {
            Debug.LogError($"{nameof(_abilityManagers)} was null or empty in {gameObject.name}");
        }

        _cell.Image.enabled = true;

        if (!isChampionUI) _cell.Button.interactable = true;

        _cell.Image.sprite = _ability.Icon;

        // Add Event bindings to button pressed
        _cell.Button.onClick.RemoveAllListeners();

        _cell.Button.onClick.AddListener(() =>
        {
            foreach (AbilityManager _abilityManager in _abilityManagers)
            {
                int abilityIndex = _abilityManager.AbilityTabs[tabIndex].Abilities.IndexOf(_ability);
                if (abilityIndex >= 0)
                {
                    _abilityManager.TryCastAbility(abilityIndex);
                }
            }
        });
    }

    private void SetPageCell(AbilityCell _cell, int _pageIndex)
    {
        _cell.Image.enabled = true;
        _cell.Button.interactable = true;

        _cell.Image.sprite = (_pageIndex > pageIndex) ? forwardSprite: backSprite;

        _cell.Button.onClick.RemoveAllListeners();

        _cell.Button.onClick.AddListener(() =>
        {
            this.SetPage(_pageIndex);
        });
    }

    public void SetPage(int _newPageIndex)
    {
        pageIndex = _newPageIndex;

        RefreshGrid();
    }

    public void SetTab(int _newTabIndex)
    {
        if (_newTabIndex < 0)
        {
            Debug.LogError("Tab index cannot be negative.");
            return;
        }

        tabIndex = _newTabIndex;
        pageIndex = 0;
        RefreshGrid();
    }

    /// <summary>
    /// Resets the selected abilities and tabs
    /// </summary>
    public void ResetSelection() // TODO: The amount of repeated code here is insane
    {
        pageIndex = 0;
        tabIndex = 0;
        commonAbilityTabs = new List<AbilityTab>();
        RefreshTabButtons();
        RefreshGrid();
        
    }

    public void RefreshTabButtons()
    {
        // Disable all tab buttons
        foreach (GameObject _tabButton in abilityTabButtons)
        {
            _tabButton.SetActive(false);
        }

        if (commonAbilityTabs == null || commonAbilityTabs.Count == 0)
        {
            return;
        }

        // Enable all tabs we have
        for (int i = 0; i < commonAbilityTabs.Count; i++)
        {
            abilityTabButtons[i].SetActive(true);
            abilityTabButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = commonAbilityTabs[i].tabName;
        }
    }

    public void UpdateGridWithUnitSelection(List<SelectableObject> _selectedUnits)
    {
        pageIndex = 0;
        //commonAbilities = GetCommonAbilities(_selectedUnits);
        commonAbilityTabs = GetCommonAbilityTabs(_selectedUnits);
        abilityManagers = _selectedUnits.Select(i => i.AbilityManager).ToList();
        RefreshTabButtons();
        RefreshGrid();
    }

    /// <summary>
    /// Updates the ability grid with the abilities of the passed in ability manager
    /// </summary>
    /// <param name="_abilityManager"></param>
    public void UpdateGridWithAbilityManager(AbilityManager _abilityManager)
    {
        pageIndex = 0; // TODO: Unsure about setting it to zero straight up?
        //commonAbilities = _abilityManager.AbilityTabs[tabIndex].Abilities;
        commonAbilityTabs = _abilityManager.AbilityTabs;
        abilityManagers = new List<AbilityManager>() { _abilityManager };

        RefreshTabButtons();
        RefreshGrid();
    }

    /// <summary>
    /// Recalculates the ability grid based on the current page and tab index and the common abilities of selected units.
    /// </summary>
    private void RefreshGrid()
    {
        ResetAbilityGrid();

        // Calculate how many abilities have already been shown on previous pages.
        // pageNumber is 0, then we have no skipped abilities.
        // pageNumber is 1, then we have 3 + 0 = 3 skipped abilities, as 1 cell is used for nav buttons.
        // pageNumber is 2, then we have 3 + 2 = 5 skipped abilities, as 3 cells are used for nav button.
        int skippedAbilities = 0;

        if (pageIndex > 0)
        {
            skippedAbilities = 3 + (pageIndex - 1) * (abilityCells.Count - 2);
        }

        Queue<Ability> abilitiesInPage = 
        new Queue<Ability>(commonAbilities.GetRange(skippedAbilities, commonAbilities.Count - skippedAbilities));

        //int abilitiesRemaining = abilitiesInPage.Count;

        for (int cellIndex = 0; cellIndex < abilityCells.Count && abilitiesInPage.Count > 0; cellIndex++)
        {
            switch (cellIndex) // TODO this is hard coed for 4 cells perhaps just do a case for first and last then add a default for the rest?
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
    /// Returns a list of ability tabs that are common across the parsed list of units by running through each tab and checking for common abilities.
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    private List<AbilityTab> GetCommonAbilityTabs(List<SelectableObject> _units)
    {
        if (_units == null || _units.Count == 0)
        {
            Debug.LogError("Cannot get common ability tabs from an empty or null unit list.");
            return new List<AbilityTab>();
        }
        List<AbilityTab> outputCommonAbilityTabs = _units[0].AbilityManager.AbilityTabs;

        if (_units.Count == 1) // If there's only 1 unit no need to scan for common
        {
            return outputCommonAbilityTabs;
        }

        // Eliminate uncommon tabs in all other units to our list of commonAbilityTabs
        for (int i = 1; i < _units.Count; i++)
        {
            SelectableObject unit = _units[i];

            // Iterate backwards to correctly remove unfound ability tabs while looping
            for (int x = outputCommonAbilityTabs.Count - 1; x >= 0; x--)
            {
                // Check 0: Check if it exists

                if (unit.AbilityManager.AbilityTabs.Count - 1 < x)
                {
                    outputCommonAbilityTabs.RemoveAt(x);
                    continue;
                }

                // Check 1: Check for name match

                if (unit.AbilityManager.AbilityTabs[x].tabName != outputCommonAbilityTabs[x].tabName)
                {
                    outputCommonAbilityTabs.RemoveAt(x);
                    continue;
                }

                // Check 2: Check for common abilities in the tab

                outputCommonAbilityTabs[x].OverrideList(GetCommonAbilities(_units, x));

                // Check 3: If the tab has no abilities left, remove it

                if (outputCommonAbilityTabs[x].Abilities.Count == 0)
                {
                    outputCommonAbilityTabs.RemoveAt(x);
                    continue;
                }
            }
        }

        return outputCommonAbilityTabs;
    }

    /// <summary>
    /// Returns a list of abilities that are common across the parsed list of units
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    private List<Ability> GetCommonAbilities(List<SelectableObject> _units, int _tabIndex = 0)
    {
        if (_units == null || _units.Count == 0)
        {
            Debug.LogError("Cannot get common abilities from an empty or null unit list.");
            return new List<Ability>();
        }

        List<Ability> commonAbilities = _units[0].AbilityManager.AbilityTabs[_tabIndex].Abilities;

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
                if (!unit.AbilityManager.AbilityTabs[_tabIndex].Abilities.Contains(commonAbilities[x]))
                {
                    commonAbilities.RemoveAt(x);
                }
            }
        }

        return commonAbilities;
    }

    
}
