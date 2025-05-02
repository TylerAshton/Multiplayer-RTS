using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Ability", menuName = "Abilities/Summon")]
public class SummonAbility : Ability
{
    [SerializeField] private GameObject spawnee;
    [SerializeField] private GameObject spawnVFX;
    [SerializeField] private float maxDispersion = 5f;
    [SerializeField] private float minDisperstion = 5f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    public override void Activate(GameObject user, Animator _animator)
    {
        Vector3 castPosition = user.transform.position + offset;

        // Generate random XZ offset within the specified dispersion range
        float offsetX = Random.Range(-maxDispersion, maxDispersion);
        float offsetZ = Random.Range(-maxDispersion, maxDispersion);

        // Ensure offset is not too close
        Vector2 offsetXZ = new Vector2(offsetX, offsetZ);
        if (offsetXZ.magnitude < minDisperstion)
        {
            offsetXZ = offsetXZ.normalized * minDisperstion;
        }

        Vector3 spawnPosition = new Vector3(
            castPosition.x + offsetXZ.x,
            castPosition.y,
            castPosition.z + offsetXZ.y
        );
        GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();
    }

    public override void DebugDrawing(GameObject _user, List<Transform> _abilityPositions)
    {

    }

    public override void OnUse(GameObject _user, List<Transform> _abilityPositions)
    {
        throw new System.NotImplementedException();
    }
}
