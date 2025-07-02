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
    public virtual void Init()
    {
        if (unit != null)
        {
            Debug.LogError("Unit is already initialized. Cannot re-initialize.");
            return;
        }

        if (!TryGetComponent<Unit>(out unit))
        {
            Debug.LogError($"{nameof(Unit)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
    }

    /// <summary>
    /// Called on update for the Unit.
    /// </summary>
    public abstract void Tick();
}
