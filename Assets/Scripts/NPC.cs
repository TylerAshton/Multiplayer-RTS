using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// NPCs are mobile units which use the nav mesh.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPC : Unit, IAbilityUser
{
    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;
    private Transform target;
    public Transform Target => target;
    private Health targetHealth;
    public Health TargetHealth => targetHealth;
    private NetCodeAnimationManager nAnimator;
    public NetCodeAnimationManager NAnimator => nAnimator;

    private AbilityPositionManager abilityPositionManager;
    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    private Collider colliderComp;

    [SerializeField] private NPCBehaviour npcBehaviour;

    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;
    public Transform Transform => transform;
    public IFaction IFaction => this;


    protected override void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!TryGetComponent<NetCodeAnimationManager>(out nAnimator))
        {
            Debug.LogError("NetCodeAnimationManager is required for NPC");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("AbilityManager is required for NPC");
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError("AbilityPositionManager is required for NPC");
        }
        if (!TryGetComponent<Collider>(out colliderComp))
        {
            Debug.LogError("Collider is required for NPC");
        }
        if (npcBehaviour == null)
        {
            Debug.LogError("NPCBehaviour is not assigned. Please assign it in the inspector.");
        }
        
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        if (!NetworkManager.Singleton.IsServer) return;

        agent.updateRotation = false;
        npcBehaviour.Init(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;

        npcBehaviour.Update(this, Time.deltaTime);
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


    

    /// <summary>
    /// Sets the nav agent's destination to the give position
    /// </summary>
    /// <param name="_worldPosition"></param>
    public void SetDestination(Vector3 _worldPosition)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to set destination for NPC");
            return;
        }

        agent.SetDestination(_worldPosition);
    }

    /// <summary>
    /// Sets the gameobject parsed as the Target, while also subscribing to it's onDeath event to the ClearTarget function
    /// </summary>
    /// <param name="_targetGameObject"></param>
    public void SetTarget(GameObject _targetGameObject)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to set target for NPC");
            return;
        }

        if (_targetGameObject == null)
        {
            // Reset tagetHealth if the we already have a target
            if (targetHealth != null)
            {
                targetHealth.OnDeath -= ClearTarget;
                targetHealth = null;
            }
            
            target = null;

            return;
        }

        if (_targetGameObject.TryGetComponent<Health>(out Health health))
        {
            targetHealth = health;
            target = _targetGameObject.transform;
            targetHealth.OnDeath -= ClearTarget;  // Ensure no duplicate subscriptions
            targetHealth.OnDeath += ClearTarget;

            npcBehaviour.OnSetTarget(this);
        }
        else
        {
            Debug.LogWarning($"{_targetGameObject.name} does not have a Health component.");
        }

        
    }

    /// <summary>
    /// Unsubscribes from the target's OnDeath event and clears all target variables
    /// </summary>
    private void ClearTarget()
    {
        targetHealth.OnDeath -= ClearTarget;
        targetHealth = null;
        target = null;

        npcBehaviour.OnClearTarget(this);
    }
}
