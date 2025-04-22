using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

/// <summary>
/// NPCs are mobile units which use the nav mesh.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPC : Unit
{
    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float detectionRange;
    [SerializeField] private float fireCoolDown;
    public float FireCoolDown => fireCoolDown;
    [SerializeField] public float fireTime;
    private Transform target;
    private Health targetHealth;
    private Animator animator;
    private AbilityManager abilityManager;
    public Health TargetHealth => targetHealth;
    protected override void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for NPC");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("AbilityManager is required for NPC");
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


        if (!target)
        {
            ScanForTarget();
        }
        else
        {
            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, detectionRange);
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
    /// Attempts to find Champion within the detectionRange and sets it as the 
    /// Target before entering the AttackState should one exist within range
    /// </summary>
    public void ScanForTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, unitLayer);

        foreach (Collider collider in hits)
        {
            if (collider.CompareTag("Champion"))
            {
                if (collider.TryGetComponent<Health>(out var _health))
                {
                    if (_health.IsDying) // TODO: Sloppy af
                    {
                        continue;
                    }
                }
                SetTarget(collider.gameObject);
            }
        }
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
    private void SetTarget(GameObject _targetGameObject)
    {
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
