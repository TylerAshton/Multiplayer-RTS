using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "Abilities/Projectile")]
public class ProjectileAbility : Ability<IAbilityUser>
{
    //[SerializeField] private GameObject projectile;
    [SerializeField] private ProjectileStats projectileStats;
    protected override string animationTrigger => "ProjectileAbility";
    protected override void ActivateTyped(IAbilityUser _user)
    {
        if (_user is ICharacterAbilityUser _characterAbilityUser)
        {
            _characterAbilityUser.NAnimator.SetTrigger($"{animationTrigger}");
        }
        else
        {
            OnUseTyped(_user);
        }
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject spawnedProjectile = Instantiate(GetProjectileBlueprint(), castPositionTransform.position, Quaternion.identity); // TODO: Change the index of ability positions and in fact how we store said positions. Dict?
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        BulletProjectile bulletProjectile = spawnedProjectile.GetComponent<BulletProjectile>();
        bulletProjectile.ApplyProjectileStatsWithID(projectileStats.ID);

        if (_user.AimPoint != Vector3.zero) // TODO: I might make this mandatory in the future once we add aim assist later to players
        {
            bulletProjectile.LaunchProjectileAtTarget(_user.AimPoint);
        }
        else
        {
            bulletProjectile.LaunchProjectile(_user.Transform.forward);
        }

        spawnedProjectile.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        DrawStat<BaseAbilityStat>(_so, "projectileStats");
    }
#endif

    private GameObject GetProjectileBlueprint()
    {
        GameObject projectile = Resources.Load<GameObject>("Blueprints/BPBullet");
        return projectile;
    }
}
