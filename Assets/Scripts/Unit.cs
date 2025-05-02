using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health), typeof(Collider))]
/// <summary>
/// Unit component is an base class used for all forms of RTS units across the game.
/// Contains logic for all common behaviours such as selection, death, and instructions.
/// </summary>
public class Unit : NetworkBehaviour, IDestructible
{
    private Queue<Task> taskQueue = new();
    private Task currentTask;
    private AbilityManager abilityManager;
    public AbilityManager AbilityManager => abilityManager;
    public Task CurrentTask => currentTask;
    [SerializeField] GameObject selectionIndiator;
    MeshRenderer selectionRenderer;
    RTSPlayer rts_Player;
    Health health;
    Collider colliderComp;
    NetworkObject networkObject;
    private bool isSelectable = true;
    public bool IsSelectable => isSelectable;

    protected virtual void Awake()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Unit selection indicator is missing");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for Unit");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("AbilityManager is required for Unit");
        }

        selectionRenderer = selectionIndiator.GetComponent<MeshRenderer>();
        health = GetComponent<Health>();
        colliderComp = GetComponent<Collider>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (RTSPlayer.instance == null)
        {
            Debug.LogError("RTS Manager doesn't exist, shutting down");
            return;
        }

        rts_Player = RTSPlayer.instance;
        rts_Player.UnitManager.AddUnit(this);
    }

    /// <summary>
    /// Sets if unit is selectable bool
    /// </summary>
    /// <param name="_isSelectable"></param>
    public void SetIsSelectable(bool _isSelectable)
    {
        isSelectable = _isSelectable;
    }

    /// <summary>
    /// Runs the Update function in the currentTask should it exist
    /// </summary>
    private void UpdateTask()
    {
        if (currentTask == null)
        {
            return;
        }

        currentTask.Update();
    }

    /// <summary>
    /// When the current task is completed exit the task. 
    /// Exit the task and start the next task if there is one in the queue
    /// </summary>
    /// <param name="_completedTask"></param>
    private void OnTaskComplete(Task _completedTask)
    {
        if (TryStartNextTask())
        {
            return;
        }

        CancelCurrentTask();
    }

    /// <summary>
    /// If there is a task in the taskQueue run SetCurrentTask with that task, returns true if successful
    /// </summary>
    private bool TryStartNextTask()
    {
        if (!taskQueue.TryDequeue(out Task nextTask))
        {
            return false;
        }

        SetCurrentTask(nextTask);
        return true;
    }

    /// <summary>
    /// Clears all tasks from the que and imposes a brand new task that starts immediately
    /// </summary>
    /// <param name="_newTask"></param>
    public void ImposeNewTask(Task _newTask)
    {
        taskQueue.Clear();
        SetCurrentTask(_newTask);
    }

    /// <summary>
    /// Exits the currentTask and sets it to null
    /// </summary>
    public void CancelCurrentTask()
    {
        if (currentTask == null)
        {
            Debug.LogError("Attempted to cancel current task when it doesn't exist");
            return;
        }

        currentTask.Exit();
        currentTask.OnTaskCompleted -= OnTaskComplete;
        currentTask = null;
    }

    /// <summary>
    /// Enqueues the parsed task into the taskQueue of the unit
    /// </summary>
    /// <param name="_newTask"></param>
    public void QueueNewTask(Task _newTask)
    {
        taskQueue.Enqueue(_newTask);
    }

    /// <summary>
    /// Sets the currentTask to the parsed Task, exiting the prexisting currentTask should it exist
    /// </summary>
    /// <param name="_task"></param>
    private void SetCurrentTask(Task _task)
    {
        if (_task == null)
        {
            Debug.LogError("Attempted to setCurrentTask with a null task");
            return;
        }

        if (currentTask != null)
        {
            CancelCurrentTask();
        }

        currentTask = _task;
        currentTask.OnTaskCompleted += OnTaskComplete;
        currentTask.Start();
    }



    // Update is called once per frame
    protected virtual void Update()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsServer) 
        {
            return;
        }

        UpdateTask();
    }

    /// <summary>
    /// Shows the glowing sphere above the unit
    /// </summary>
    public virtual void ShowSelectionIndicator()
    {
        selectionIndiator.SetActive(true);
    }

    /// <summary>
    /// Hides the glowing sphere above the unit
    /// </summary>
    public virtual void HideSelectionIndicator()
    {
        selectionIndiator.SetActive(false);
    }

    /// <summary>
    /// Changes the colour of the glowing sphere above the unit, used for DEBUG purposes
    /// </summary>
    /// <param name="_color"></param>
    public void SetSelectionColor(Color _color)
    {
        selectionRenderer.material.color = _color;
    }

    public virtual void DestroyObject()
    {
        if (rts_Player)
        {
            rts_Player.UnitManager.RemoveUnit(this);
        }      
    }

    /// <summary>
    /// Returns a Vector3 of the lowest point of the object in the centre
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFeet()
    {
        Bounds bounds = colliderComp.bounds;

        Vector3 lowestPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        return lowestPoint;
    }
}
