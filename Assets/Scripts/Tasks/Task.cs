using System;
using UnityEngine;

public abstract class Task
{
    protected Unit unit;
    protected Color stateDebugColor = Color.black;
    public Color StateDebugColor => stateDebugColor;
    public event Action<Task> OnTaskCompleted;
    private bool hasCompleted = false; // Used as a safeguard against completing twice

    public Task(Unit _unit)
    {
        unit = _unit;
    }

    /// <summary>
    /// Called when the Task is began
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Called every frame when the Task is active
    /// </summary>
    public virtual void Update()
    {
        TryComplete();
    }

    /// <summary>
    /// Called when the Task has stopped
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// Called during update to see if the Task is complete.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsComplete();

    /// <summary>
    /// Checks if the task is complete, if so notifying the unit to exit the task.
    /// NOTE: Will not be ran if the task is ended outside of this.
    /// </summary>
    protected void TryComplete()
    {
        if (hasCompleted)
        {
            Debug.LogError("Attempted to complete a task that is alreadey completed");
            return;
        }

        if (IsComplete())
        {
            hasCompleted = true;
            OnTaskCompleted?.Invoke(this);
        }
    }
}
