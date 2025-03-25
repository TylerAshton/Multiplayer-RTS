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
    [SerializeField] GameObject selectionIndiator;
    MeshRenderer selectionRenderer;
    RTSPlayer rts_Player;
    Health health;
    Collider colliderComp;
    NetworkObject networkObject;

    [SerializeField] State currentState;

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
            Debug.LogError("RTS Manager doesn't exist");
            return;
        }

        currentState = new IdleState(this);
        currentState.Enter();

        rts_Player = RTSPlayer.instance;
        rts_Player.UnitManager.AddUnit(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (currentState != null)
        {
            currentState.Update();
        }
    }

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
    }

    public virtual void ShowSelectionIndicator()
    {
        selectionIndiator.SetActive(true);
    }

    public virtual void HideSelectionIndicator()
    {
        selectionIndiator.SetActive(false);
    }

    public void SetSelectionColor(Color _color)
    {
        selectionRenderer.material.color = _color;
    }

    public virtual void DestroyObject()
    {
        rts_Player.UnitManager.RemoveUnit(this);
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
