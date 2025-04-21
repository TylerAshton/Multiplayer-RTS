using UnityEngine;

public abstract class Task
{
    protected Unit unit;
    protected Color stateDebugColor = Color.black;
    public Color StateDebugColor => stateDebugColor;

    public Task(Unit _unit)
    {
        unit = _unit;
    }

    /// <summary>
    /// Called when the Task is began
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Called every frame when the Task is active
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Called when the Task is ending
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// Called during update to see if the Task is complete.
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsComplete();

    /// <summary>
    /// Called once the task is complete, will not be ran if the task is ended outside of this.
    /// </summary>
    protected abstract void OnComplete();
}
