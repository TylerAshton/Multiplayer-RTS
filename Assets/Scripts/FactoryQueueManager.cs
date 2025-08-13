using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConstructionItem
{
    private ConstructionStats constructionStats;
    public ConstructionStats ConstructionStats => constructionStats;
    private int cost;
    public int Cost => cost;
    private bool isPaid;
    public bool IsPaid => isPaid;

    public ConstructionItem(ConstructionStats _constructionStats, int _cost, bool _isPaid)
    {
        constructionStats = _constructionStats;
        cost = _cost;
        isPaid = _isPaid;
    }

    public void SetPaid(bool _value)
    {
        isPaid = _value;
    }
}

public class FactoryQueueManager : NetworkBehaviour
{
    [SerializeField] private ConstructionProgressBar progressBar;
    private Queue<ConstructionItem> productionQueue = new Queue<ConstructionItem>();
    public Queue<ConstructionItem> ProductionQueue => productionQueue;
    private ConstructionItem currentProduction;
    private AbilityManager abilityManager;
    private AbilityPositionManager abilityPositionManager;
    private bool isRepeating => abilityManager.IsUtilityEnabled;

    private void Awake()
    {
        if(!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError($"{GetType().Name} requires a {nameof(AbilityManager)} component.");
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError($"{GetType().Name} requires a {nameof(AbilityPositionManager)} component.");
        }
    }

    public void EnqueueUnit(ConstructionItem _constructionItem)
    {
        if (!IsServer)
        {
            Debug.LogWarning($"{nameof(EnqueueUnit)} can only be called on the server.");
            return;
        }
/*        Debug.Log($"{_constructionItem.name} has been enqueued for production.");*/

        // Add the construction stats to the queue
        productionQueue.Enqueue(_constructionItem);


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
        
        currentProduction = productionQueue.Peek();

        progressBar.gameObject.SetActive(true);
        progressBar.Slider.maxValue = currentProduction.ConstructionStats.ConstructionTime;
        StartCoroutine(ProduceCurrentUnit());
    }

    private IEnumerator ProduceCurrentUnit()
    {
        while (!currentProduction.IsPaid)
        {
            currentProduction.SetPaid(TryPurchaseConstruction(currentProduction));
            yield return null;
        }

        Vector3 spawnPos = CalculateSpawnPos(currentProduction.ConstructionStats);
        //SpawnCurrentProductionSummonVfxRpc(spawnPos);
        VFXSpawner.Instance.SpawnVfxObjectRpc(currentProduction.ConstructionStats.SummonVfx.ID, spawnPos, 99);

        float timeElapsed = 0f;
        float duration = currentProduction.ConstructionStats.ConstructionTime;

        while (timeElapsed < duration)
        {
            progressBar.Slider.value = timeElapsed;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        SpawnCurrentProduction(spawnPos);
        VFXSpawner.Instance.SpawnVfxObjectRpc(currentProduction.ConstructionStats.SpawnVfx.ID, spawnPos, 99);
        //SpawnCurrentProudctionSpawnVfxRpc(spawnPos);

        productionQueue.Dequeue();
        // Requeue the current production if the production is set to repeat
        if (isRepeating)
        {
            ConstructionItem constructionItem = new ConstructionItem(
                currentProduction.ConstructionStats,
                currentProduction.Cost,
                false
            );

            TryPurchaseConstruction(constructionItem);

            productionQueue.Enqueue(constructionItem);
        }
        currentProduction = null;

        // Start the next production if there are units in the queue
        if (productionQueue.Count > 0)
        {
            StartProduction();
        }
        else
        {
            progressBar.gameObject.SetActive(false);
        }
    }

    private bool TryPurchaseConstruction(ConstructionItem _constructionItem)
    {
        if (_constructionItem == null)
        {
            Debug.LogError("Tried to purchase item that was null!");
            return false;
        }
        if (_constructionItem.IsPaid)
        {
            Debug.LogError("Tried to purchase and item that was already purchased");
            return false;
        }

        ulong id = NetworkManager.LocalClientId;
        int points = PointManager.Instance.GetPoints(id);

        if (points >= _constructionItem.Cost)
        {
            PointManager.Instance.RemovePoints(id, _constructionItem.Cost);
            _constructionItem.SetPaid(true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns the actual prefab of the current production at the given position.
    /// </summary>
    /// <param name="_spawnPos"></param>
    private void SpawnCurrentProduction(Vector3 _spawnPos)
    {
        GameObject summoned = Instantiate(currentProduction.ConstructionStats.ConstructablePrefab, _spawnPos, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();
    }

    /*/// <summary>
    /// Spawns vfx of the current production at the given position.
    /// </summary>
    /// <param name="_spawnPos"></param>
    [Rpc(SendTo.Everyone)]
    private void SpawnCurrentProudctionSpawnVfxRpc(Vector3 _spawnPos)
    {
        

        GameObject spawnedVfx = Instantiate(currentProduction.SpawnVFX, _spawnPos, Quaternion.identity);
        VFXScaler.ScaleParticles(currentProduction.SpawnVFXScale, spawnedVfx);

        Destroy(spawnedVfx, currentProduction.VfxDespawnTime);
    }

    /// <summary>
    /// Summon a looping vfx at the spawn position of the current production. Used to represent portals
    /// </summary>
    /// <param name="_spawnPos"></param>
    [Rpc(SendTo.Everyone)]
    private void SpawnCurrentProductionSummonVfxRpc(Vector3 _spawnPos)
    {
        GameObject spawnedVfx = Instantiate(currentProduction.SummonVFX, _spawnPos, Quaternion.identity);
        VFXScaler.ScaleParticles(currentProduction.SummonVFXScale, spawnedVfx);
        Destroy(spawnedVfx, currentProduction.ConstructionTime);
    }*/



    /// <summary>
    /// Returns the position where the unit should be spawned based on the construction stats.
    /// </summary>
    /// <param name="_constructionStats"></param>
    /// <returns></returns>
    private Vector3 CalculateSpawnPos(ConstructionStats _constructionStats)
    {
        Vector3 castPosition = abilityPositionManager.AbilityPositions[AbilityPosition.Centre].position + _constructionStats.Offset;

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
