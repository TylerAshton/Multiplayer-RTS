using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AmalgamUpgradeManager : NetworkBehaviour
{
    private int upgradeLevel = 1;
    [SerializeField] private List<ConstructionPad> tier2BuildPads = new List<ConstructionPad>();
    [SerializeField] private List<ConstructionPad> tier3BuildPads = new List<ConstructionPad>();


    private void Start()
    {
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
        foreach (var pad in tier2BuildPads)
        {
            pad.isUnlocked = true;
        }
    }

    public void UpgradeTier3()
    {
        foreach (var pad in tier3BuildPads)
        {
            pad.isUnlocked = true;
        }
    }
}
