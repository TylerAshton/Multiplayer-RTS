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
    private NetCodeAnimationManager nAnimator;

    Collider colliderComp;
    public Health TargetHealth => targetHealth;

    public NetCodeAnimationManager NAnimator => nAnimator;

    public Transform Transform => transform;

    private AbilityPositionManager abilityPositionManager;

    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;

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

        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        if (!NetworkManager.Singleton.IsServer) return;

        agent.updateRotation = false; // I'M IN CHARGE NOW BITCH
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;

        UpdateRotation();
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


    private void UpdateRotation()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), Time.deltaTime);
        }

        else if (agent.velocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(agent.velocity.normalized), Time.deltaTime);
        }       
    }

    /// <summary>
    /// Sets the nav agent's destination to the give position
    /// </summary>
    /// <param name="_worldPosition"></param>
    public void SetDestination(Vector3 _worldPosition)
    {
        agent.SetDestination(_worldPosition);
    }

    /// <summary>
    /// Fires the pojectile towards the target's position
    /// </summary>
    public void Shoot()
    {
        abilityManager.TryCastAbility(0);
    }

    /// <summary>
    /// Sets the gameobject parsed as the Target, while also subscribing to it's onDeath event to the ClearTarget function
    /// </summary>
    /// <param name="_targetGameObject"></param>
    public void SetTarget(GameObject _targetGameObject)
    {
        if (_targetGameObject == null)
        {
            targetHealth.OnDeath -= ClearTarget;
            targetHealth = null;
            target = null;

            return;
        }

        if (_targetGameObject.TryGetComponent<Health>(out Health health))
        {
            targetHealth = health;
            target = _targetGameObject.transform;
            targetHealth.OnDeath -= ClearTarget;  // Ensure no duplicate subscriptions
            targetHealth.OnDeath += ClearTarget;
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
    }
}
