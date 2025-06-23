using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Assists NPCs in finding and targeting Champions within a specified range.
/// </summary>
public class NPCTargettingManager : NetworkBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private float forgivenessRange;
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

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        TryForgiveTarget();

        if (!npc.Target)
        {
            ScanForTarget();
        }
    }

    private void TryForgiveTarget()
    {
        if (!npc.Target)
        {
            return;
        }

        float targetDistance = Vector3.Distance(npc.Target.position, npc.transform.position);

        if (targetDistance > forgivenessRange)
        {
            npc.SetTarget(null);
        }
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
