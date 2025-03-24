using UnityEngine;

public abstract class State
{
    protected Unit unit;
    protected Color stateDebugColor = Color.black;
    public Color StateDebugColor => stateDebugColor;

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

    /// <summary>
    /// Called during update to see if it should change to idle state. Used for task states
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsComplete();

    protected abstract void OnComplete();
}
