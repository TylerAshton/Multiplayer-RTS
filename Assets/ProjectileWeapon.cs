using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileWeapon : Weapon
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform firePosition;


    public override void Attack()
    {
        SpawnBulletServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc()
    {
        Debug.Log("Yaa");
        GameObject spawnedProjectile = Instantiate(projectile, firePosition.position, Quaternion.identity);
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        spawnedProjectile.GetComponent<BulletProjectile>().LaunchProjectile(transform.forward);
    }

    public override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Debug attack function for testing purposes only
    /// </summary>
    /// <param name="context"></param>
    public void INPUTAttack(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        Attack();
    }
}
