using Unity.Netcode;
using UnityEngine;

public class ReviveSoul : NetworkBehaviour, IInteractable
{
    public InteractionPopUp InteractionPopUp => interactionPopUp;
    [SerializeField] private InteractionPopUp interactionPopUp;
    [SerializeField] ReviveColourChange reviveColourChange;
    private NetworkObject networkObject;

    private Health soulHealth;

    public void Init(Health _health)
    {
        soulHealth = _health;
        networkObject = GetComponent<NetworkObject>();
    }

    public void Interact()
    {
        InteractionRpc();
    }

    [Rpc(SendTo.Server)]
    private void InteractionRpc()
    {
        soulHealth.ReviveObject();
        networkObject.Despawn();
    }

    public void ShowProgress(float _progress)
    {
        reviveColourChange.SetParticleColour(_progress);
    }
}
