using UnityEngine;

public interface IDestructible
{
    /// <summary>
    /// Runs any custom destruction logic an object has that should be ran before it's destroyed.
    /// NOTE: This doesn't destroy the gameobject, it only PREPARES. Please use this in tandem with
    /// a function like Health.DestroyObject
    /// </summary>
    void DestroyObject();
}
