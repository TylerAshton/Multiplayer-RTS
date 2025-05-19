using Unity.Netcode;
using UnityEngine;

public class AutoAttackController : NetworkBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private LayerMask unitLayer;
    private NPC npc;
    private AbilityManager abilityManager;

    private void Awake()
    {
        if (!TryGetComponent<NPC>(out npc))
        {
            Debug.LogError("NPC is required for AutoAttackController");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("abilityManager is required for AutoAttackController");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        TryAttackTarget();

        if (!npc.Target)
        {
            ScanForTarget();
        }
    }

    private void TryAttackTarget()
    {
        if (!npc.Target)
        {
            return;
        }
        
        if (!CanAttackTarget())
        {
            return;
        }

        abilityManager.TryCastAbility(0);

    }

    private bool CanAttackTarget() // TODO MAKE ME
    {
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, detectionRange);
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
                npc.SetTarget(collider.gameObject);
            }
        }
    }
}
