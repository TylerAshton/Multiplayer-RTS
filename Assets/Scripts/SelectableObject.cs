using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// SelectableObject component is an base class used for all forms of RTS units across the game.
/// Contains logic for all common behaviours such as selection, and instructions.
/// </summary>
public class SelectableObject : NetworkBehaviour, IFaction, IAbilityUser
{
    private Queue<Task> taskQueue = new();
    private Task currentTask;
    protected AbilityManager abilityManager;
    public AbilityManager AbilityManager => abilityManager;
    public Task CurrentTask => currentTask;
    [SerializeField] private GameObject selectionIndiator;
    private MeshRenderer selectionRenderer;
    protected RTSPlayer rts_Player;
    
    [SerializeField] private bool isSelectable = true;
    public bool IsSelectable => isSelectable;

    protected Faction faction = Faction.Amalgam;
    Faction IFaction.Faction { get => faction; set => faction = value; }

    private AbilityPositionManager abilityPositionManager;
    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    private Transform castTarget;
    public Transform CastTarget => castTarget;
    public Vector3 AimPoint => castTarget != null ? castTarget.position : Vector3.zero;

    public Transform Transform => transform;

    public IFaction IFaction => this;


    private Health castTargetHealth;

    protected virtual void Awake()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError($"Unit selection indicator is required for {GetType().Name} on gameobject: {gameObject.name}");
        }

        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError($"{nameof(AbilityManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError($"{nameof(AbilityPositionManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
        }

        selectionRenderer = selectionIndiator.GetComponent<MeshRenderer>();
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
    private void OnCurrentTaskComplete()
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
        if (_newTask == null)
        {
            Debug.LogError("Attempted to impose a new task that is null");
            return;
        }

        if (currentTask != null)
        {
            CancelCurrentTask();
        }

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
        currentTask.OnTaskCompleted -= OnCurrentTaskComplete;
        currentTask = null;
    }

    /// <summary>
    /// Enqueues the parsed task into the taskQueue of the unit
    /// </summary>
    /// <param name="_newTask"></param>
    public void QueueNewTask(Task _newTask)
    {
        if (_newTask == null)
        {
            Debug.LogError("Attempted to queue a new task that is null");
            return;
        }

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
        currentTask.OnTaskCompleted += OnCurrentTaskComplete;
        currentTask.Start();
    }



    // Update is called once per frame
    protected virtual void Update()
    {
        if (!IsServer) 
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
        if (selectionIndiator == null)
        {
            Debug.LogError("Selection indicator is null!");
            return;
        }

        selectionIndiator.SetActive(true);
    }

    /// <summary>
    /// Hides the glowing sphere above the unit
    /// </summary>
    public virtual void HideSelectionIndicator()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Selection indicator is null!");
            return;
        }
        selectionIndiator.SetActive(false);
    }

    /// <summary>
    /// Changes the colour of the glowing sphere above the unit, used for DEBUG purposes
    /// </summary>
    /// <param name="_color"></param>
    public void SetSelectionColor(Color _color)
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Selection indicator is null!");
            return;
        }
        selectionRenderer.material.color = _color;
    }
    /// <summary>
    /// Sets the gameobject parsed as the Target, while also subscribing to it's onDeath event to the ClearTarget function
    /// </summary>
    /// <param name="_newTarget"></param>
    public void SetTarget(Transform _newTarget) // TODO: Move all setTarget shit to Unit
    {
        if (!IsServer)
        {
            Debug.LogError($"Client attempted to set target for {nameof(NPC)}");
            return;
        }

        if (_newTarget == null)
        {
            Debug.LogError($"_newTarget cannot be null in {nameof(SetTarget)}. Use {nameof(ClearTarget)} instead if this was intentional!");
            return;
        }

        castTarget = _newTarget;

        if (_newTarget.TryGetComponent<Health>(out Health health))
        {
            castTargetHealth = health;
            castTargetHealth.OnDeath -= ClearTarget;  // Ensure no duplicate subscriptions
            castTargetHealth.OnDeath += ClearTarget;
        }
    }

    /// <summary>
    /// Unsubscribes from the target's OnDeath event and clears all target variables
    /// </summary>
    public void ClearTarget()
    {
        if (!IsServer)
        {
            Debug.LogError($"Client attempted to use {nameof(ClearTarget)} for {nameof(NPC)}");
            return;
        }

        castTargetHealth.OnDeath -= ClearTarget;
        castTargetHealth = null;
        castTarget = null;
    }
}
