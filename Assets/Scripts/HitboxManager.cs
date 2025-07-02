using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages hitboxes for abilities, usually projections. But should be modular enough to handle any hitbox type.
/// </summary>
public class HitboxManager : MonoBehaviour
{
    private HitboxStats hitboxStats;
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
/*        HitboxStats hitboxStats = ProjectileStatsRegistry.GetProjectileStat(_hitboxStatsID);

        if (hitboxStats == null)
        {
            Debug.LogError("ProjectileStats is null");
            return;
        }

        if (!hitboxStats.IsValid())
        {
            Debug.LogError("ProjectileStats is not valid, check the console for more information");
            return;
        }*/



        /*detectionRange = _projectileStats.DetectionRange;
        speed = _projectileStats.Speed;
        damage = _projectileStats.Damage;
        lifeTime = _projectileStats.LifeTime;
        bulletVFX = _projectileStats.BulletVFX;
        bulletVFXScale = _projectileStats.BulletVFXScale;
        deathVFX = _projectileStats.DeathVFX;
        deathVFXScale = _projectileStats.DeathVFXScale;
        isAOE = _projectileStats.IsAOE;
        aoeRadius = _projectileStats.AOERadius;
        penetration = _projectileStats.Penetration;
*/
        //SpawmBulletVFXRpc();
    }

    public void ApplyHitboxStatsWithID(string _hitboxStatsID)
    {
        ApplyHitboxStatsRpc(_hitboxStatsID);
    }
}
