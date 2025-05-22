using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Construct Ability", menuName = "Abilities/Construct")]
public class Construct : Ability<IConstructionPad>
{
    [SerializeField] private GameObject spawnee;
    [SerializeField] private GameObject spawnVFX;
    protected override void ActivateTyped(IConstructionPad _user)
    {
        Vector3 spawnPosition = _user.Transform.position;


        GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        vfx.GetComponent<NetworkObject>().Spawn();
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();

        _user.ConstructionPad.HideBuildPad();
        summoned.GetComponent<Health>().OnDeath += _user.ConstructionPad.ShowBuildPad;

        // Select new unit
        RTSPlayer.instance.UnitManager.SelectUnit(summoned.GetComponent<Unit>());
    }

    protected override void DebugDrawingTyped(IConstructionPad _user)
    {
        
    }

    protected override void OnUseTyped(IConstructionPad _user)
    {
        
    }
}
