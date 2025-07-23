using UnityEngine;

/// <summary>
/// This interface provides access to the AmalgamUpgradeManager used only in the AmalgamCore.
/// </summary>
public interface IAmalgamCore : IFactory
{
    AmalgamUpgradeManager AmalgamUpgradeManager { get;}
}
