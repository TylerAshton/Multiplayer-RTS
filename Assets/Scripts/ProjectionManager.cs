using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages projection ability instances in the game.
/// </summary>
public class ProjectionManager : NetworkBehaviour, IFaction
{
    private ProjectionStats projectionStats;
    HitboxManager hitboxManager;
    List<Collider> hitColliders = new List<Collider>();

    public Faction Faction { get => faction; set => faction = value; }
    private Faction faction = Faction.None;

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

        hitboxManager.OnHitboxTriggerStay += OnHit;
    }

    private void OnHit(Collider _other)
    {
        if (!_other.TryGetComponent<IFaction>(out IFaction _faction))
        {
            return;
        }

        if (_faction.Faction != faction)
        {
            return;
        }

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
            _health.Damage(projectionStats.Damage);
            hitColliders.Add(_other);
        }
        else
        {
            _health.Damage(projectionStats.Damage * Time.deltaTime);
        }
    }


    [Rpc(SendTo.Everyone)]
    private void ApplyProjectionStatsRpc(string _projectionStatsID)
    {
        ProjectionStats projectionStats = AbilityStatsRegistry.GetProjectileStat<ProjectionStats>(_projectionStatsID);

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

    public void ApplyHitboxStatsWithID(string _projectionStatsID)
    {
        ApplyProjectionStatsRpc(_projectionStatsID);
    }
}
