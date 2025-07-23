using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawns vfx on death
/// </summary>
public class DeathVfxSpawner : NetworkBehaviour
{
    private Health health;

    private NetworkObject networkObject;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private float vfxScale = 1f;
    [SerializeField] private float lingertTime = 1f;

    private void Awake()
    {
        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError($"{nameof(Health)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError($"{nameof(NetworkObject)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
        if (vfxPrefab == null)
        {
            Debug.LogError($"{nameof(vfxPrefab)} is required for {GetType().Name} on gameobject: {gameObject.name}");
            return;
        }
        if (vfxScale <= 0f)
        {
            Debug.LogError($"{nameof(vfxScale)} cannot be none or negative in: {gameObject.name}");
            return;
        }
        if (lingertTime <= 0f)
        {
            Debug.LogError($"{nameof(lingertTime)} cannot be none or negative in: {gameObject.name}");
            return;
        }
    }

    private void OnEnable()
    {
        health.OnDeath += SpawnVFXRpc;
    }

    private void OnDisable()
    {
        health.OnDeath -= SpawnVFXRpc;
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnVFXRpc()
    {
        Debug.Log(networkObject.IsSceneObject);

        GameObject spawnedVfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        VFXScaler.ScaleParticles(vfxScale, spawnedVfx);

        Destroy(spawnedVfx, lingertTime);
    }
}
