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

    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;
    private Health targetHealth;
    public Health TargetHealth => targetHealth;


    private void Awake()
    {
        if (!TryGetComponent<Unit>(out unit))
        {
            Debug.LogError($"{nameof(Unit)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
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

        if (!currentTarget)
        {
            TryScanForTarget();
        }
    }

    private void TryForgiveTarget()
    {
        if (!currentTarget)
        {
            return;
        }

        float targetDistance = Vector3.Distance(currentTarget.position, unit.transform.position);

        if (targetDistance > forgivenessRange)
        {
            SetTarget(null);
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
        if (currentTarget)
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
                SetTarget(collider.gameObject);
            }
        }
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

            currentTarget = null;

            return;
        }

        if (_targetGameObject.TryGetComponent<Health>(out Health health))
        {
            targetHealth = health;
            currentTarget = _targetGameObject.transform;
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
        currentTarget = null;
    }
}
