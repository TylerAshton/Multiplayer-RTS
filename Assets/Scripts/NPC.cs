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

    private NetCodeAnimationManager nAnimator;
    public NetCodeAnimationManager NAnimator => nAnimator;



    private Collider colliderComp;

    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;
    public Transform Transform => transform;
    public IFaction IFaction => this;

    private Transform castTarget;
    public Transform CastTarget => castTarget;

    private Health castTargetHealth;
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

        agent.updateRotation = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;
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

    /*    /// <summary>
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
            }
            else
            {
                Debug.LogWarning($"{_targetGameObject.name} does not have a Health component.");
            }


        }*/

    /*    /// <summary>
        /// Unsubscribes from the target's OnDeath event and clears all target variables
        /// </summary>
        private void ClearTarget()
        {
            targetHealth.OnDeath -= ClearTarget;
            targetHealth = null;
            target = null;
        }*/
}
