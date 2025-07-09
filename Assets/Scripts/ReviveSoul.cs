using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ReviveSoul : NetworkBehaviour, IInteractable
{
    public InteractionPopUp InteractionPopUp => interactionPopUp;
    [SerializeField] private InteractionPopUp interactionPopUp;
    [SerializeField] ReviveColourChange reviveColourChange;
    private NetworkObject networkObject;

    private Health soulHealth;
    private GameObject playerGameObject;
    private NetworkTransform playerNTransform;

    [SerializeField] private float respawnTime = 5f;
    private float respawnCounter = 0;
    private bool isRespawning = false;

    private Transform respawnPos;

    public void Init(GameObject _playerGameObject)
    {
        if (_playerGameObject == null)
        {
            Debug.LogError($"{nameof(_playerGameObject)} can't be null!");
            return;
        }

        if (!_playerGameObject.TryGetComponent<Health>(out soulHealth))
        {
            Debug.LogError($"{GetType().Name} requires a {nameof(Health)} component which isn't present on {_playerGameObject.name}!");
            return;
        }

        if (!_playerGameObject.TryGetComponent<NetworkTransform>(out playerNTransform))
        {
            Debug.LogError($"{GetType().Name} requires a {nameof(NetworkTransform)} component which isn't present on {_playerGameObject.name}!");
        }

        playerGameObject = _playerGameObject;
        networkObject = GetComponent<NetworkObject>();


        respawnPos = LobbyManager.Instance.PChampionSpawnPos;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (isRespawning)
        {
            return;
        }

        respawnCounter += Time.deltaTime;
        if (respawnCounter >= respawnTime)
        {
            playerNTransform.Teleport(respawnPos.position, playerGameObject.transform.rotation, playerGameObject.transform.localScale);
            RespawnRpc();
        }
    }

    public void Interact()
    {
        RespawnRpc();
    }

    [Rpc(SendTo.Server)]
    private void RespawnRpc()
    {
        if (isRespawning == true)
        {
            Debug.LogWarning($"{nameof(RespawnRpc)} was called whilst already respawning");
            return;
        }

        isRespawning = true;
        soulHealth.ReviveObject();
        networkObject.Despawn();
    }

    public void ShowProgress(float _progress)
    {
        reviveColourChange.SetParticleColour(_progress);
    }
}
