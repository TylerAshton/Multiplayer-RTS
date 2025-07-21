using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FactoryQueueManager : NetworkBehaviour
{
    private Queue<ConstructionStats> productionQueue = new Queue<ConstructionStats>();
    private ConstructionStats currentProduction;

    public void EnqueueUnit(ConstructionStats _constructionStats)
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{nameof(EnqueueUnit)} can only be called on the server.");
            return;
        }
        // Logic to enqueue a unit for production
        Debug.Log($"{_constructionStats.name} has been enqueued for production.");
        //productionQueue.Enqueue(_constructionStats);
        currentProduction = _constructionStats;
        StartCoroutine(ProduceUnit());

    }

    private void StartProduction()
    {
        if (currentProduction != null)
        {
            Debug.LogError("Production is already in progress.");
            return;
        }
    }

    private IEnumerator ProduceUnit()
    {
        yield return new WaitForSeconds(currentProduction.ConstructionTime);
        GameObject summoned = Instantiate(currentProduction.ConstructablePrefab, transform.position, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();
    }

    private void Update()
    {

    }
}
