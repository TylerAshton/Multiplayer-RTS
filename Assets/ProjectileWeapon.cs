using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform firePosition;
    public override void Attack()
    {
        GameObject spawnedProjectile = Instantiate(projectile, firePosition.position, transform.rotation);
    }

    public override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }
}
