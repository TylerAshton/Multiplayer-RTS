using UnityEngine;

/// <summary>
/// Interface for what faction the character is on: WRITEABLE
/// </summary>
public interface IChangeableFaction : IFaction
{
    public new Faction Faction { get; set; }
}
