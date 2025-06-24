using UnityEngine;

/// <summary>
/// Reusable behaviour for units in the game.
/// </summary>
public abstract class UnitBehaviour : MonoBehaviour
{
    protected Unit unit;

    /// <summary>
    /// Called to initialize the UnitBehaviour with a Unit.
    /// </summary>
    /// <param name="_unit"></param>
    public virtual void Init(Unit _unit)
    {
        if (unit != null)
        {
            Debug.LogError("Unit is already initialized. Cannot re-initialize.");
            return;
        }

        if (_unit == null)
        {
            Debug.LogError("_unit cannot be null");
            return;
        }
        unit = _unit;
    }

    /// <summary>
    /// Called on update for the Unit.
    /// </summary>
    public abstract void Tick();
}
