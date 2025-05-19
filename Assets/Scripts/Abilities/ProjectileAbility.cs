using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "Abilities/Projectile")]
public class ProjectileAbility : Ability<IAbilityUser>
{
    [SerializeField] private GameObject projectile;
    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.Animator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject spawnedProjectile = Instantiate(projectile, castPositionTransform.position, Quaternion.identity); // TODO: Change the index of ability positions and in fact how we store said positions. Dict?
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        spawnedProjectile.GetComponent<BulletProjectile>().LaunchProjectile(_user.Transform.forward);
    }
}
