using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPCs are mobile units which use the nav mesh.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPC : Unit, ICharacterAbilityUser
{
    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    private Collider colliderComp;
    private AnimationTriggerManager animTriggerManager;
    public AnimationTriggerManager AnimTriggerManager => animTriggerManager;
    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;





    protected override void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        
        if (!TryGetComponent<Collider>(out colliderComp))
        {
            Debug.LogError("Collider is required for NPC");
        }
        if (!TryGetComponent<AnimationTriggerManager>(out animTriggerManager))
        {
            Debug.LogError($"{nameof(AnimationTriggerManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
        }
        if (!TryGetComponent<EffectManager>(out effectManager))
        {
            Debug.LogError($"{nameof(EffectManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
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

    // TODO: Perhaps make a base character class as this is shared with Champions
    public void Lunge(float distance, Vector3 direction, float duration)
    {
        StartCoroutine(LungeRoutine(distance, direction.normalized, duration));
    }

    private IEnumerator LungeRoutine(float distance, Vector3 direction, float duration)
    {
        float elapsed = 0f;
        float speed = distance / duration;
        agent.updatePosition = false;

        while (elapsed < duration)
        {
            float step = speed * Time.deltaTime;
            agent.Move(direction * step);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.updatePosition = true;
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
