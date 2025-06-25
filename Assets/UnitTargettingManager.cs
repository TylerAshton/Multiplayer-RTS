using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Assists NPCs in finding and targeting Champions within a specified range.
/// </summary>
public class UnitTargettingManager : NetworkBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private float forgivenessRange;
    [SerializeField] private LayerMask unitLayer;
    private Unit unit;
    private AbilityManager abilityManager;

    private IAbilityUser abilityUser;


    private void Awake()
    {
        if (!TryGetComponent<Unit>(out unit))
        {
            Debug.LogError($"{nameof(Unit)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
        abilityUser = unit as IAbilityUser;
        if (abilityUser == null)
        {
            Debug.LogError($"{nameof(IAbilityUser)} is required for {GetType().Name} on {gameObject.name}");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError($"{nameof(AbilityManager)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
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

        if (!abilityUser.CastTarget)
        {
            TryScanForTarget();
        }
    }

    private void TryForgiveTarget()
    {
        if (!abilityUser.CastTarget)
        {
            return;
        }

        float targetDistance = Vector3.Distance(abilityUser.CastTarget.position, unit.transform.position);

        if (targetDistance > forgivenessRange)
        {
            abilityUser.ClearTarget();
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
    public void TryScanForTarget()
    {
        // Only run if the NPC does not have a target
        if (abilityUser.CastTarget)
        {
            return; 
        }

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
                abilityUser.SetTarget(collider.transform);
            }
        }
    }
}

    

