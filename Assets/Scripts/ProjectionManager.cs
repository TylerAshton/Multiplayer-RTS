using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages projection ability instances in the game.
/// </summary>
public class ProjectionManager : NetworkBehaviour
{
    private ProjectionStats projectionStats;
    HitboxManager hitboxManager;
    List<Collider> hitColliders = new List<Collider>();
    GameObject spawnedVfx;

    

    private void Init()
    {
        if (!IsServer) // I'm quite sure all of this behaviour is server-side only.
        {
            return;
        }

        if (!TryGetComponent<HitboxManager>(out hitboxManager))
        {
            Debug.LogError($"{nameof(HitboxManager)} component is missing on {gameObject.name}. Cannot initialize {GetType().Name}!");
            return;
        }

        SpawnProjectionVFXRpc();

        hitboxManager.OnHitboxTriggerStay += OnHit;

        StartCoroutine(deathTime());
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnProjectionVFXRpc()
    {
        if (projectionStats  == null)
        {
            return;
        }

        if (projectionStats.VFXPrefab == null)
        {
            return;
        }

        spawnedVfx = Instantiate(projectionStats.VFXPrefab, transform);
        spawnedVfx.transform.position += projectionStats.VFXOffset;
        spawnedVfx.transform.rotation *= Quaternion.Euler(projectionStats.VFXRotation.eulerAngles);
    }

    private IEnumerator deathTime()
    {
        yield return new WaitForSeconds(projectionStats.Duration);
        Destroy(spawnedVfx);
        GetComponent<NetworkObject>().Despawn();

    }

    private void OnHit(Collider _other)
    {        
        if (projectionStats.DamageOnce && hitColliders.Contains(_other))
        {
            return;
        }

        if (!_other.TryGetComponent<Health>(out Health _health))
        {
            return;
        }

        if (projectionStats.DamageOnce)
        {
            hitColliders.Add(_other);
        }

        _health.Damage(projectionStats.Damage);
    }


    [Rpc(SendTo.Everyone)]
    private void ApplyProjectionStatsRpc(string _projectionStatsID)
    {
        ProjectionStats projectionStats = Registry<ProjectionStats>.GetItem(_projectionStatsID);

        if (projectionStats == null)
        {
            Debug.LogError($"{nameof(projectionStats)} is null. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }

        if (!projectionStats.IsValid())
        {
            Debug.LogError($"{nameof(projectionStats)} is not valid. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }

        this.projectionStats = projectionStats;
        
        Init();
    }

    public void ApplyProjectionStatsWithID(string _projectionStatsID)
    {
        ApplyProjectionStatsRpc(_projectionStatsID);
    }
}
