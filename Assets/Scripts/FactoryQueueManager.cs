using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        Debug.Log($"{_constructionStats.name} has been enqueued for production.");

        // Add the construction stats to the queue
        productionQueue.Enqueue(_constructionStats);


        // If there's no queue, start production immediately
        if (currentProduction == null)
        {
            StartProduction();
            return;
        }

        
    }

    private void StartProduction()
    {
        if (currentProduction != null)
        {
            Debug.LogError("Production is already in progress.");
            return;
        }

        currentProduction = productionQueue.Dequeue();
        StartCoroutine(ProduceCurrentUnit());
    }

    private IEnumerator ProduceCurrentUnit()
    {
        Vector3 spawnPos = CalculateSpawnPos(currentProduction);
        yield return new WaitForSeconds(currentProduction.ConstructionTime);



        GameObject summoned = Instantiate(currentProduction.ConstructablePrefab, spawnPos, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();

        currentProduction = null;

        // Start the next production if there are units in the queue
        if (productionQueue.Count > 0)
        {
            StartProduction();
        }
    }

    /// <summary>
    /// Returns the position where the unit should be spawned based on the construction stats.
    /// </summary>
    /// <param name="_constructionStats"></param>
    /// <returns></returns>
    private Vector3 CalculateSpawnPos(ConstructionStats _constructionStats)
    {
        Vector3 castPosition = transform.position + _constructionStats.Offset;

        // Generate random XZ offset within the specified dispersion range
        float offsetX = Random.Range(-_constructionStats.MaxDispersion, _constructionStats.MaxDispersion);
        float offsetZ = Random.Range(-_constructionStats.MaxDispersion, _constructionStats.MaxDispersion);

        // Ensure offset is not too close
        Vector2 offsetXZ = new Vector2(offsetX, offsetZ);
        if (offsetXZ.magnitude < _constructionStats.MinDisperstion)
        {
            offsetXZ = offsetXZ.normalized * _constructionStats.MinDisperstion;
        }

        Vector3 spawnPosition = new Vector3(
            castPosition.x + offsetXZ.x,
            castPosition.y,
            castPosition.z + offsetXZ.y
        );

        return spawnPosition;
    }

    private void Update()
    {

    }
}
