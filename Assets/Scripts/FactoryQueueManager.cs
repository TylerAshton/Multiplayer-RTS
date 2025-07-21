using Unity.Netcode;
using UnityEngine;

public class FactoryQueueManager : NetworkBehaviour
{
    public void EnqueueUnit()
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{nameof(EnqueueUnit)} can only be called on the server.");
            return;
        }
        // Logic to enqueue a unit for production
        Debug.Log("Unit has been enqueued for production.");
    }
}
