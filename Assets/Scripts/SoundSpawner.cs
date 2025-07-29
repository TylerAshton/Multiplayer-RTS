using FMODUnity;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class SoundSpawner : NetworkBehaviour
{
    public static SoundSpawner Instance { get; private set; }

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
    public void PlaySoundEffectRpc(string _vfxObjectID, Vector3 _pos) // TODO: Make it play without position for the owner if desired
    {
        SoundObject soundObject = Registry<SoundObject>.GetItem(_vfxObjectID);

        if (soundObject == null)
        {
            Debug.LogError($"SoundObject '{_vfxObjectID}' not found in the registry.");
            return;
        }

        if (soundObject.SoundEvent.IsNull)
        {
            Debug.LogError($"SoundObject '{_vfxObjectID}' not found or has no sound event.");
            return;
        }

        RuntimeManager.PlayOneShot(soundObject.SoundEvent, _pos);
    }
}
