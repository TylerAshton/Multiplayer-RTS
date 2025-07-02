using Unity.Netcode;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class manages hitboxes for abilities, usually projections. But should be modular enough to handle any hitbox type.
/// </summary>
public class HitboxManager : MonoBehaviour
{
    public void Init(HitboxStats _hitboxStats)
    {
        if (_hitboxStats == null)
        {
            Debug.LogError($"{nameof(_hitboxStats)} is null. Cannot initialize {GetType().Name} in gameobject - {gameObject.name}!.");
            return;
        }
    }

    /// <summary>
    /// Applies the projectile stats to the bullet, this is used to set the stats of the bullet when it is instantiated
    /// </summary>
    /// <param name="_projectileStats"></param>
    /// 
    [Rpc(SendTo.Everyone)]
    private void ApplyHitboxStatsRpc(string _hitboxStatsID)
    {
        HitboxStats hitboxStats = AbilityStatsRegistry.GetProjectileStat<HitboxStats>(_hitboxStatsID);

        if (hitboxStats == null)
        {
            Debug.LogError("ProjectileStats is null");
            return;
        }

        if (!hitboxStats.IsValid())
        {
            Debug.LogError("ProjectileStats is not valid, check the console for more information");
            return;
        }

        switch (hitboxStats.HitboxType) // I think type conditionals are fine. Doing derived classes would be overkill for this.
        {
            case HitboxType.Sphere:
                SpawnSphere(hitboxStats);
                break;
            case HitboxType.Box:
                SpawnBox(hitboxStats);
                break;
            case HitboxType.Cone:
                SpawnCone(hitboxStats);
                break;
            default:
                EditorGUILayout.HelpBox("Unknown hitbox type!", MessageType.Error);
                break;
        }
    }

    private void SpawnSphere(HitboxStats _hitboxStats)
    {
        SphereCollider sCollider = gameObject.AddComponent<SphereCollider>();
        sCollider.isTrigger = true;
        sCollider.center = _hitboxStats.Offset;
        sCollider.radius = _hitboxStats.SphereStartRadius;
    }
    private void SpawnBox(HitboxStats _hitboxStats)
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = _hitboxStats.Offset;
        boxCollider.size = _hitboxStats.BoxStartSize;
    }
    private void SpawnCone(HitboxStats _hitboxStats)
    {
        SpawnSphere(_hitboxStats);
    }

    public void ApplyHitboxStatsWithID(string _hitboxStatsID)
    {
        ApplyHitboxStatsRpc(_hitboxStatsID);
    }
}
