using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Construct Ability", menuName = "Abilities/Construct")]
public class Construct : Ability
{
    [SerializeField] private GameObject spawnee;
    [SerializeField] private GameObject spawnVFX;
    public override void Activate(GameObject user, Animator _animator)
    {
        Vector3 spawnPosition = user.transform.position;


        GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();

        user.GetComponent<ConstructionPad>().HideBuildPad();
        summoned.GetComponent<Health>().OnDeath += user.GetComponent<ConstructionPad>().ShowBuildPad;
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {
        
    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        
    }
}
