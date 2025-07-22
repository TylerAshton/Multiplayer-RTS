using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AmalgamUpgradeManager : NetworkBehaviour
{

    private AbilityManager abilityManager;
    private int upgradeLevel = 1;
    [SerializeField] private int abilityTabTarget = 0; // Abilities should be on the first tab
    [SerializeField] private List<ConstructionPad> tier2BuildPads = new List<ConstructionPad>();
    [SerializeField] private List<Ability> tier2Abilities = new List<Ability>();
    [SerializeField] private List<ConstructionPad> tier3BuildPads = new List<ConstructionPad>();
    [SerializeField] private List<Ability> tier3Abilities = new List<Ability>();

    private void Awake()
    {
        if(!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError($"{GetType().Name} requires an {nameof(AbilityManager)} component in gameobjhect {gameObject.name}!");
            return;
        }
    }

    private void Start()
    {
        if (!IsServer)
        {
            return;
        }

        foreach (var pad in tier2BuildPads)
        {
            pad.isUnlocked = false;
        }

        foreach (var pad in tier3BuildPads)
        {
            pad.isUnlocked = false;
        }
    }

    public void Upgrade()
    {
        if (!IsServer)
        {
            Debug.LogError("Upgrade can only be called on the server.");
            return;
        }

        if (upgradeLevel >= 3)
        {
            Debug.LogWarning("Max upgrade tier already reached.");
            return;
        }

        upgradeLevel++;

        switch (upgradeLevel)
        {
            case 2:
                UpgradeTier2();
                break;
            case 3:
                UpgradeTier3();
                break;
            default:
                Debug.LogWarning("No further upgrades available.");
                break;
        }
    }

    public void UpgradeTier2()
    {
        if (!IsServer)
        {
            Debug.LogError("UpgradeTier2 can only be called on the server.");
            return;
        }

        foreach (ConstructionPad _pad in tier2BuildPads)
        {
            _pad.isUnlocked = true;
        }

        foreach (Ability _ability in tier2Abilities)
        {
            abilityManager.AddAbility(_ability, abilityTabTarget);
        }
    }

    public void UpgradeTier3()
    {
        if (!IsServer)
        {
            Debug.LogError("UpgradeTier3 can only be called on the server.");
            return;
        }

        foreach (var pad in tier3BuildPads)
        {
            pad.isUnlocked = true;
        }

        foreach (var ability in tier3Abilities)
        {
            abilityManager.AddAbility(ability, abilityTabTarget);
        }
    }
}
