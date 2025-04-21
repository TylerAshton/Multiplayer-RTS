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
    [SerializeField] GameObject selectionIndiator;
    MeshRenderer selectionRenderer;
    RTSPlayer rts_Player;
    Health health;
    Collider colliderComp;
    NetworkObject networkObject;

    //[SerializeField] State currentState;

    protected virtual void Awake()
    {
        if (selectionIndiator == null)
        {
            Debug.LogError("Unit selection indicator is missing");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
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

    private void UpdateTask()
    {
        if (currentTask == null)
        {
            return;
        }

        currentTask.Update();
    }

    public void ImposeNewTask(Task _newTask)
    {
        SetCurrentTask(_newTask);
    }

    private void SetCurrentTask(Task _task)
    {
        if (currentTask != null)
        {
            currentTask.Exit();
        }

        currentTask = _task;
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

/*    /// <summary>
    /// Exits the current state if one exists, before entering the new state.
    /// </summary>
    /// <param name="_newState"></param>
    public void ChangeState(State _newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = _newState;
        currentState.Enter();

        #if UNITY_EDITOR
            SetSelectionColor(_newState.StateDebugColor);
        #endif
    }*/

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
