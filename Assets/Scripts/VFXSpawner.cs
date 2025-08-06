using Unity.Netcode;
using UnityEngine;

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

    [Rpc(SendTo.Everyone)]
    public void SpawnVfxObjectRpc(string _vfxObjectID, Vector3 _pos)
    {
        VfxObject vfxObject = Registry<VfxObject>.GetItem(_vfxObjectID);

        if (vfxObject == null)
        {
            Debug.LogError($"VfxObject '{_vfxObjectID}' not found.");
            return;
        }

        GameObject spawnedVfx = Instantiate(vfxObject.VfxPrefab, _pos + vfxObject.VfxOffset, Quaternion.identity);
        VFXScaler.ScaleParticles(vfxObject.VfxScale, spawnedVfx);

        Destroy(spawnedVfx, vfxObject.LingerTime);
    }
}
