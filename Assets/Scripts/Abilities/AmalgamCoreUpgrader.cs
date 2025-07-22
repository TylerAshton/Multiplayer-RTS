using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This is a hard coded ability that upgrades the Amalgam Core.
/// </summary>
public class AmalgamCoreUpgrader : Ability<IAmalgamCore>
{
    private const int _tabIndex = 2; // upgrades should be on 3rd tab

    protected override void ActivateTyped(IAmalgamCore _user)
    {
        _user.AmalgamUpgradeManager.Upgrade();

        if (Successor != null)
        {
            _user.AbilityManager.AddAbility(Successor, _tabIndex);
        }

        else
        {
            _user.AbilityManager.RemoveAbility(this);
        }    
    }

    protected override void DebugDrawingTyped(IAmalgamCore _user)
    {

    }

    protected override void OnUseTyped(IAmalgamCore _user)
    {

    }
}
