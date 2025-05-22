using UnityEngine;


public enum Faction
{
    Champion,
    Amalgam
}

/// <summary>
/// Interface for what faction the character is on: READONLY
/// </summary>
public interface IFaction
{
    public Faction Faction { get; }
}
