using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;


public interface VfxObject
{
    GameObject VfxPrefab { get; }
    Vector3 VfxOffset { get; }
    float VfxScale { get; }
    float VfxDuration { get; }
}

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
    public void AbilityVfxRpc(string _abilityID, Vector3 _parentPos)
    {
        Ability ability = Registry<Ability>.GetItem(_abilityID);

        if (ability == null)
        {
            Debug.LogError($"Ability '{_abilityID}' not found.");
            return;
        }

        if (ability is VfxObject vfxObject)
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
}
