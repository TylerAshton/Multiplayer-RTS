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
    [SerializeField] private GameObject projectile;
    private Transform target;
    private Health targetHealth;
    public Health TargetHealth => targetHealth;
    protected override void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, detectionRange);
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
                    if(_health.IsDying) // TODO: Sloppy af
                    {
                        continue;
                    }
                }
                SetTarget(collider.gameObject);
                AttackState attackState = new AttackState(this);
                ChangeState(attackState);
            }
        }
    }

    /// <summary>
    /// Fires the pojectile towards the target's position
    /// </summary>
    public void Shoot()
    {
        Debug.Log("pew");
        Vector3 direction = (target.position - transform.position).normalized;

        GameObject newProjectile = (GameObject)Instantiate(projectile, transform.position, Quaternion.LookRotation(direction));

        // Register over network
        NetworkObject bulletNetwork = newProjectile.GetComponent<NetworkObject>();
        bulletNetwork.Spawn();
        newProjectile.SetActive(true);

        BulletProjectile _projectile = newProjectile.GetComponent<BulletProjectile>();
        _projectile.Fire();
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
