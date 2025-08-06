using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum AbilityCellType
{
    Unset,
    Ability,
    Page
}

public class AbilityCell : UIActionCell
{
    private const double abilPartialThreshold = 0.6;

    [SerializeField] private Slider slider;

    private Ability ability;

    private List<AbilityManager> abilityManagers;

    private AbilityCellType abilityCellType = AbilityCellType.Unset;

    private AbilityUIManager abilityUIManager;

    private void Awake()
    {
        abilityUIManager = GetComponentInParent<AbilityUIManager>();

        if (abilityUIManager == null)
        {
            Debug.LogError($"AbilityUIManager not found in parent of {gameObject.name}");
        }
    }

    public void SetAbility(Ability _newAbility, List<AbilityManager> _abilityManagers, bool _interactable)
    {
        if (abilityCellType != AbilityCellType.Unset)
        {
            Debug.LogError($"Attempted to set an abilityCell that was already set as {abilityCellType}");
            return;
        }

        if (_newAbility == null)
        {
            Debug.LogError($"{nameof(_newAbility)} was null in {gameObject.name}!");
            return;
        }

        if (_abilityManagers == null || _abilityManagers.Count == 0)
        {
            Debug.LogError($"{nameof(_abilityManagers)} was null or empty in {gameObject.name}");
        }

        ability = _newAbility;
        abilityManagers = _abilityManagers;

        if (_interactable) button.interactable = true;

        image.enabled = true;
        image.sprite = _newAbility.Icon;

        // Add Event bindings to button pressed. This is server only so no need to convert for clients
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            Ability castingAbility = ability; 
            List<AbilityManager> castingAbilityManagers = new List<AbilityManager>(abilityManagers); 

            foreach (AbilityManager __abilityManager in castingAbilityManagers)
            {
                __abilityManager.TryCastAbility(castingAbility);
            }
        });

        abilityCellType = AbilityCellType.Ability;
    }

    public void SetPageCell(int _pageIndex)
    {
        if (abilityCellType != AbilityCellType.Unset)
        {
            Debug.LogError($"Attempted to set an abilityCell that was already set as {abilityCellType}");
            return;
        }

        image.enabled = true;
        button.interactable = true;

        image.sprite = (_pageIndex > abilityUIManager.PageIndex) ? abilityUIManager.ForwardSprite : abilityUIManager.BackSprite;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            abilityUIManager.SetPage(_pageIndex);
        });

        abilityCellType = AbilityCellType.Page;

    }

    /// <summary>
    /// Reset EVERYTHING in this cell.
    /// </summary>
    public void ResetCell()
    {
        ability = null;
        abilityManagers = null;
        image.enabled = false;
        image.sprite = null;
        slider.value = 0;
        button.onClick.RemoveAllListeners();
        button.interactable = false;
        image.color = Color.white;
        abilityCellType = AbilityCellType.Unset;
    }


    public void OnUpdate()
    {
        if (abilityCellType == AbilityCellType.Ability)
        {
            ShowAvailability();
            ShowCooldown();
        }
        
    }

    private void ShowAvailability()
    {
        float percentageAvailable = GetPercentageAvailability(ability);

        if (percentageAvailable == 1)
        {
            image.color = Color.white; // Ability is available
        }
        else if (percentageAvailable > abilPartialThreshold)
        {
            image.color = Color.yellow; // Ability is partially available
        }
        else
        {
            image.color = Color.red; // Ability is not available
        }
    }

    private void ShowCooldown()
    {
        float cooldownStartTime = GetLongestCooldown(ability);

        if (cooldownStartTime == 0)
        {
            slider.value = 0;
            return;
        }

        // Calculate remaining time until end of cooldown
        slider.maxValue = ability.Cooldown;

        float cooldownEndTime = ability.Cooldown + cooldownStartTime;
        float timePassed = Time.time - cooldownStartTime;
        float timeRemaining = ability.Cooldown - timePassed;

        slider.value = timeRemaining;
    }

    /// <summary>
    /// Returns a percentage(float) of how many ability managers can cast the parsed ability.
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private float GetPercentageAvailability(Ability _ability)
    {
        float percentageAvailable = 0f;

        int availableCount = abilityManagers.Count(am => am.CanCastAbility(_ability));

        percentageAvailable = availableCount / (float)abilityManagers.Count;

        return percentageAvailable;
    }

    /// <summary>
    /// Returns the longest cooldown value among the abilityManagers with the parsed Ability
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private float GetLongestCooldown(Ability _ability)
    {
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



}
