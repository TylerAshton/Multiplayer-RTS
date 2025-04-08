using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "Abilities/Projectile")]
public class ProjectileAbility : Ability
{
    [SerializeField] private GameObject projectile;
    public override void Activate(GameObject user, Animator _animator)
    {
        _animator.SetTrigger($"{animationTrigger}");
    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        Debug.Log("Yaa");
        GameObject spawnedProjectile = Instantiate(projectile, _abilityPositions[0].position, Quaternion.identity); // TODO: Change the index of ability positions and in fact how we store said positions. Dict?
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        spawnedProjectile.GetComponent<BulletProjectile>().LaunchProjectile(_user.transform.forward);
    }
}
