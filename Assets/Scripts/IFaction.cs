using UnityEngine;


public enum Faction
{
    None,
    Champion,
    Amalgam
}

/// <summary>
/// Interface for what faction the character is on.
/// </summary>
public interface IFaction
{
    public Faction Faction { get; set; }
}
