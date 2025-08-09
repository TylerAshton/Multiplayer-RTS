using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructionUIManager : MonoBehaviour
{
    List<Factory> factories = new List<Factory>();
    [SerializeField] private GameObject QueueCellPrefab;
    private Queue<ConstructionItem> productionQueue;
    private Queue<ConstructionItem> lastDrawnQueue;

    private void Awake()
    {
        if (QueueCellPrefab == null)
        {
            Debug.LogError($"{GetType().Name} requires a QueueCellPrefab to be assigned in gameobject {gameObject.name}!");
        }
    }

    private void Update()
    {
        if (productionQueue == null)
        {
            return;
        }

        if (lastDrawnQueue == productionQueue)
        {
            return;
        }

        if (productionQueue.Count == 0)
        {
            ClearPanel();
        }
        else
        {
            TryDrawConstructionQueue();
        }

        lastDrawnQueue = new Queue<ConstructionItem>(productionQueue);
    }

    public void UpdateUI(List<SelectableObject> _selectableObjects)
    {
        if (_selectableObjects == null || _selectableObjects.Count == 0)
        {
            Debug.LogError($"{GetType().Name} received an empty or null list of selectable objects.");
            return;
        }

        ResetManager();

        bool allAreFactories = _selectableObjects.All(obj => obj is Factory);

        if (!allAreFactories)
        {
            return;
        }

        factories = _selectableObjects.Cast<Factory>().ToList();

        productionQueue = GetConstructionQueue(factories);
        TryDrawConstructionQueue();


    }

    private void TryDrawConstructionQueue()
    {
        if (productionQueue == null || productionQueue.Count == 0)
        {
            return;
        }

        ClearPanel();

        foreach (ConstructionItem _constructionItem in productionQueue)
        {
            if (_constructionItem == null)
            {
                Debug.LogError($"{GetType().Name} received a null ConstructionStats in the production queue.");
                continue;
            }

            GameObject queueCellObject = Instantiate(QueueCellPrefab, transform);
            QueueCell queueCell;

            if (!queueCellObject.TryGetComponent<QueueCell>(out queueCell))
            {
                Debug.LogError($"{GetType().Name} could not find QueueCell script on {queueCellObject.name}");
                continue;
            }

            queueCell.SetQueueCell(_constructionItem);
        }
    }

    private Queue<ConstructionItem> GetConstructionQueue(List<Factory> factories)
    {
        if (factories == null || factories.Count == 0)
        {
            Debug.LogError($"{GetType().Name} received an empty or null list of selectable objects.");
            return null;
        }

        // Use first queue as reference
        Queue<ConstructionItem> newQueue = factories[0].FactoryQueueManager.ProductionQueue;

        bool allIdentical = factories.All(f =>f.FactoryQueueManager.ProductionQueue.SequenceEqual(newQueue)); // If all queues are identical

        if (allIdentical)
        {
            return newQueue;
        }
        else
        {
            return null;
        }
    }

    public void ClearPanel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ResetManager()
    {
        productionQueue = null;
        ClearPanel();
    }
}
