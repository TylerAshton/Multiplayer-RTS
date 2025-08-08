using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    
    [SerializeField] private UtilityCell utilityCell;
    [SerializeField] private List<AbilityCell> abilityCells = new List<AbilityCell>();
    [SerializeField] private List<GameObject> abilityTabButtons = new List<GameObject>();
    [SerializeField] private Sprite forwardSprite;
    public Sprite ForwardSprite => forwardSprite;
    [SerializeField] private Sprite backSprite;
    public Sprite BackSprite => backSprite;
    private int pageIndex = 0;
    public int PageIndex => pageIndex;
    private int tabIndex = 0;
    public int TabIndex => tabIndex;
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
        UpdateAbilityCells();
    }

    private void UpdateAbilityCells()
    {
        foreach (AbilityCell cell in abilityCells)
        {
            if (cell == null)
            {
                continue;
            }
            cell.OnUpdate();
        }
    }

    private void ResetUtilityButton()
    {
        if (utilityCell == null)
        {
            if (abilityManagers[0].HasUtility)
            {
                Debug.LogError("Utility cell is null but at least one ability manager has utility!");
            }
            return;
        }

        utilityCell.ResetCell();
    }

    /// <summary>
    /// Disables and hides all ability cells in the grid
    /// </summary>
    public void ResetAbilityGrid()
    {
        foreach (AbilityCell _cell in abilityCells)
        {
            _cell.ResetCell();
        }

        ResetUtilityButton();
    }

    /// <summary>
    /// Shows
    /// </summary>
    /// <param name="_ability"></param>
    /// <param name="_cell"></param>
    private void SetAbilityCell(Ability _ability, AbilityCell _cell, List<AbilityManager> _abilityManagers)
    {
        _cell.SetAbility(_ability, _abilityManagers, !isChampionUI);

        /*if (_ability == null)
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
                    _abilityManager.TryCastAbility(abilityIndex, tabIndex);
                }
            }
        });*/
    }

    private void SetPageCell(AbilityCell _cell, int _pageIndex)
    {
        _cell.SetPageCell(_pageIndex);
        /*        _cell.Image.enabled = true;
                _cell.Button.interactable = true;

                _cell.Image.sprite = (_pageIndex > pageIndex) ? forwardSprite: backSprite;

                _cell.Button.onClick.RemoveAllListeners();

                _cell.Button.onClick.AddListener(() =>
                {
                    this.SetPage(_pageIndex);
                });*/
    }

    public void SetPage(int _newPageIndex)
    {
        pageIndex = _newPageIndex;

        RefreshAbilityGrid();
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
        RefreshAbilityGrid();
    }

    /// <summary>
    /// Updates the ability grid and tab buttons based on the currently selected units.
    /// </summary>
    public void ClearUI()
    {
        pageIndex = 0;
        tabIndex = 0;
        commonAbilityTabs = new List<AbilityTab>();
        RefreshTabButtons();
        RefreshAbilityGrid();
        ResetUtilityButton();

    }

    /// <summary>
    /// Refreshes all ability grid, tab buttons and utility UI with the current commonAbilityTabs
    /// </summary>
    private void RefreshAll()
    {
        RefreshTabButtons();
        RefreshAbilityGrid();
        RefreshUtilityButton();
    }

    private void RecalculateCommonAbilities()
    {
        commonAbilityTabs = GetCommonAbilityTabs(abilityManagers);
        RefreshAll();
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

    public void UpdateAbilityTabsWithUnitSelection(List<SelectableObject> _selectedUnits)
    {
        pageIndex = 0;
        List<AbilityManager> newAbilityManagers = _selectedUnits.Select(i => i.AbilityManager).ToList();
        SetAbilityManagers(newAbilityManagers);
        
    }

    /// <summary>
    /// Updates the ability grid with the abilities of the passed in ability manager
    /// </summary>
    /// <param name="_abilityManager"></param>
    public void UpdateAbilityTabsWithAbilityManager(AbilityManager _abilityManager)
    {
        pageIndex = 0; // Set page index to 0 as we are changing the ability manager
        commonAbilityTabs = _abilityManager.AbilityTabs;
        abilityManagers = new List<AbilityManager>() { _abilityManager };

        RecalculateCommonAbilities();
    }
    private void SetAbilityManagers(List<AbilityManager> _abilityManagers)
    {
        if (_abilityManagers == null || _abilityManagers.Count == 0)
        {
            Debug.LogError("Cannot set ability managers to an empty or null list.");
            return;
        }

        // Unsubsribe from old list
        foreach (AbilityManager _abilityManager in abilityManagers)
        {
            _abilityManager.OnAbilitiesChanged -= RecalculateCommonAbilities;
        }

        // Subscribe to new list
        foreach (AbilityManager _abilityManager in _abilityManagers)
        {
            if (_abilityManager == null)
            {
                Debug.LogError("Cannot set a null ability manager.");
                return;
            }

            _abilityManager.OnAbilitiesChanged += RecalculateCommonAbilities;
        }

        abilityManagers = _abilityManagers;

        // Refresh UI
        RecalculateCommonAbilities();
    }

    


    private void RefreshUtilityButton()
    {
        if (utilityCell == null)
        {
            if (abilityManagers[0].HasUtility)
            {
                Debug.LogError("Utility cell is null but at least one ability manager has utility!");
            }
            return;
        }

        bool allHasUtility = abilityManagers.All(m => m.HasUtility);

        if (!allHasUtility)
        {
            ResetUtilityButton();
            return;
        }

        utilityCell.Refresh(abilityManagers, () =>
        {
            foreach (AbilityManager _abilityManager in abilityManagers)
            {
                _abilityManager.ToggleUtility();
            }
        });
    }

    /// <summary>
    /// Recalculates the ability grid based on the current page and tab index and the common abilities of selected units.
    /// </summary>
    private void RefreshAbilityGrid()
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
            if (cellIndex == 0)
            {
                if (pageIndex > 0)
                {
                    SetPageCell(abilityCells[cellIndex], pageIndex - 1);
                }
                else
                {
                    SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                }
            }

            else if (cellIndex == abilityCells.Count - 1)
            {
                if (abilitiesInPage.Count > 1)
                {
                    SetPageCell(abilityCells[cellIndex], pageIndex + 1);
                }
                else
                {
                    SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
                }
            }
            else
            {
                SetAbilityCell(abilitiesInPage.Dequeue(), abilityCells[cellIndex], abilityManagers);
            }
        }
    }

    /// <summary>
    /// Returns a list of ability tabs that are common across the parsed list of units by running through each tab and checking for common abilities.
    /// </summary>
    /// <param name="_abilityManagers"></param>
    /// <returns></returns>
    private List<AbilityTab> GetCommonAbilityTabs(List<AbilityManager> _abilityManagers)
    {

        if (_abilityManagers == null || _abilityManagers.Count == 0)
        {
            Debug.LogError("Cannot get common ability tabs from an empty or null abilityManagers list.");
            return new List<AbilityTab>();
        }

        AbilityManager firstManager = _abilityManagers[0];
        List<AbilityTab> commonTabs = firstManager.AbilityTabs;

        for (int i = 0; i < firstManager.AbilityTabs.Count; i++)
        {
            commonTabs[i].OverrideList(GetCommonAbilitiesInTab(_abilityManagers, i));
        }

        for (int i = commonTabs.Count - 1; i >= 0; i--)
        {
            if (commonTabs[i].Abilities.Count == 0)
            {
                commonTabs.RemoveAt(i);
            }
        }

        // Fail safe check for if the selected index is now out of bounds after removing tabs
        if (commonTabs.Count - 1 < tabIndex)
        {
            tabIndex = 0; 
        }

        return commonTabs;
    }

    /// <summary>
    /// Returns a list of abilities that are common across the parsed list of _abilityManagers in the specified tab index.
    /// </summary>
    /// <param name="_abilityManagers"></param>
    /// <param name="_tabIndex"></param>
    /// <returns></returns>
    private List<Ability> GetCommonAbilitiesInTab(List<AbilityManager> _abilityManagers, int _tabIndex)
    {
        List<Ability> commonAbilitiesInTab = new List<Ability>();
        if (_tabIndex < 0)
        {
            Debug.LogError("Tab index cannot be negative.");
            return commonAbilitiesInTab;
        }

        // Check if the tab exists in all ability managers
        if (_abilityManagers.Any(am => _tabIndex >= am.AbilityTabs.Count))
        {
            return commonAbilitiesInTab;
        }

        List<List<Ability>> abilityLists = _abilityManagers.Select(am => am.AbilityTabs[_tabIndex].Abilities).ToList();
        commonAbilitiesInTab = abilityLists.Aggregate((current, next) => current.Intersect(next).ToList());

        return commonAbilitiesInTab;
    }    
}
