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
