using FMODUnity;
using System.Xml.Linq;
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
    public void SpawnVfxObjectRpc(string _vfxObjectID, Vector3 _pos, ulong _castingPlayerID = ulong.MaxValue)
    {
        VfxObject vfxObject = Registry<VfxObject>.GetItem(_vfxObjectID);

        if (vfxObject == null)
        {
            Debug.LogError($"VfxObject '{_vfxObjectID}' not found.");
            return;
        }

        if (vfxObject.VfxPrefab == null)
        {
            Debug.LogError($"VfxObject: {vfxObject.name} doesn't have a vfx prefab!");
            return;
        }

        if (vfxObject.OnlyShowForCaster && _castingPlayerID != NetworkManager.LocalClientId)
        {
            return;
        }

        SpawnVfx(vfxObject, _pos);
    }

    private void SpawnVfx(VfxObject _vfxObject, Vector3 _pos)
    {
        GameObject spawnedVfx = Instantiate(_vfxObject.VfxPrefab, _pos + _vfxObject.VfxOffset, Quaternion.identity);
        VFXScaler.ScaleParticles(_vfxObject.VfxScale, spawnedVfx);

        Destroy(spawnedVfx, _vfxObject.LingerTime);
    }


    /// <summary>
    /// Plays a sound effect over the network using the provided Sound Object ID and position and optionally the casting player ID. ID of 99 means it will play for everyone.
    /// </summary>
    /// <param name="_soundObject"></param>
    /// <param name="_pos"></param>
    /// <param name="_castingPlayerID"></param>
    [Rpc(SendTo.Everyone)]
    public void PlaySoundEffectRpc(string _soundObject, Vector3 _pos, ulong _castingPlayerID = ulong.MaxValue)
    {
        SoundObject soundObject = Registry<SoundObject>.GetItem(_soundObject);

        if (soundObject == null)
        {
            Debug.LogError($"SoundObject '{_soundObject}' not found in the registry.");
            return;
        }

        if (soundObject.SoundEvent.IsNull)
        {
            Debug.LogError($"SoundObject '{_soundObject}' not found or has no sound event.");
            return;
        }


        if (soundObject.OnlyPlayForCaster)
        {
            if (_castingPlayerID == ulong.MaxValue)
            {
                Debug.LogError($"SoundObject '{_soundObject}' is set to only play for the caster, but castingPlayerID wasn't specified. This is not allowed!");
                return;
            }

            if (_castingPlayerID == NetworkManager.LocalClientId)
            {
                PlaySound(soundObject, _pos, _castingPlayerID);
            }
        }

        else
        {
            PlaySound(soundObject, _pos, _castingPlayerID);
        }




    }

    private void PlaySound(SoundObject _soundObject, Vector3 _pos, ulong _castingPlayerID)
    {
        if (_soundObject.Has2DVariant)
        {
            if (_castingPlayerID == ulong.MaxValue)
            {
                Debug.LogError($"SoundObject '{_soundObject.ID}' is set to play 2D sound for the caster, but castingPlayerID was not specified. This is not allowed!");
                return;
            }

            if (_castingPlayerID == NetworkManager.LocalClientId)
            {
                // If the sound event parsed is NOT 2D this will silently fail and just play in the abyss
                EventReference soundData = (_soundObject.SoundEvent2D.Guid.IsNull) ? _soundObject.SoundEvent : _soundObject.SoundEvent2D;

                RuntimeManager.PlayOneShot(soundData);
            }
        }

        else
        {
            RuntimeManager.PlayOneShot(_soundObject.SoundEvent, _pos);
        }
    }
}
