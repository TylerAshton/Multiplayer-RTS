using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This is a hard coded ability that upgrades the Amalgam Core.
/// </summary>
public class AmalgamCoreUpgrader : Ability<IAmalgamCore>
{

    protected override void OnCastTyped(IAmalgamCore _user)
    {
        _user.AmalgamUpgradeManager.Upgrade();

        if (Successor != null)
        {
            int tabIndex = _user.AbilityManager.FindAbilityTabIndex(this);
            _user.AbilityManager.AddAbility(Successor, tabIndex);
        }

        else
        {
            _user.AbilityManager.RemoveAbility(this);
        }    
    }

    protected override void DebugDrawingTyped(IAmalgamCore _user)
    {

    }

    protected override void OnApexTyped(IAmalgamCore _user)
    {

    }
}
