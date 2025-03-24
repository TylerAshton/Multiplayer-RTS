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

    public void Shoot()
    {
        Debug.Log("pew");
        Vector3 direction = (target.position - transform.position).normalized;

        GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.LookRotation(direction));

        BulletProjectile _projectile = newProjectile.GetComponent<BulletProjectile>();
        _projectile.Fire();
    }

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

    private void ClearTarget()
    {
        targetHealth.OnDeath -= ClearTarget;
        targetHealth = null;
        target = null;
    }
}
