using UnityEngine;

public abstract class State
{
    protected Unit unit;

    public State(Unit _unit)
    {
        unit = _unit;
    }

    /// <summary>
    /// Called whenever the state is began
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Called every frame when the state is active
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Called when the state is ending
    /// </summary>
    public abstract void Exit();
}
