using Unity.Netcode;
using UnityEngine;

public class CleaveMeleeWeapon : Weapon
{
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwingCleaveServerRpc()
    {

    }

    public override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }
}
