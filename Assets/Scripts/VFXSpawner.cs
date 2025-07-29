using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;


public interface IVfxObject
{
    GameObject VfxPrefab { get; }
    Vector3 VfxOffset { get; }
    float VfxScale { get; }
    float VfxDuration { get; }
}

/// <summary>
/// Static manager that spawns vfx over the network in cases where it cannot be managed by another script
/// </summary>
public class VFXSpawner : NetworkBehaviour
{
    public static VFXSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void RAWR()
    {
        Debug.Log("called");
        MEERpc();

    }

    [Rpc(SendTo.Everyone)]
    private void MEERpc()
    {
        Debug.Log("SERVER");
    }

    [Rpc(SendTo.Everyone)]
    public void SpawnAbilityVfxRpc(string _abilityID, Vector3 _parentPos) // TODO: Remove this
    {
        Ability ability = Registry<Ability>.GetItem(_abilityID);

        if (ability == null)
        {
            Debug.LogError($"Ability '{_abilityID}' not found.");
            return;
        }

        if (ability is IVfxObject vfxObject)
        {
            if (vfxObject.VfxPrefab == null)
            {
                Debug.LogError($"{nameof(vfxObject.VfxPrefab)} is null in {ability.name}");
                return;
            }

            GameObject spawnedVfx = Instantiate(vfxObject.VfxPrefab, _parentPos + vfxObject.VfxOffset, Quaternion.identity);
            VFXScaler.ScaleParticles(vfxObject.VfxScale, spawnedVfx);

            Destroy(spawnedVfx, vfxObject.VfxDuration);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SpawnVfxObjectRpc(string _vfxObjectID, Vector3 _pos)
    {
        VfxObject vfxObject = Registry<VfxObject>.GetItem(_vfxObjectID);

        if (vfxObject == null)
        {
            Debug.LogError($"VfxObject '{_vfxObjectID}' not found.");
            return;
        }

        GameObject spawnedVfx = Instantiate(vfxObject.VfxPrefab, _pos, Quaternion.identity);
        VFXScaler.ScaleParticles(vfxObject.VfxScale, spawnedVfx);

        Destroy(spawnedVfx, vfxObject.LingerTime);
    }
}
