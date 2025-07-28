using UnityEngine;

public class Factory : Building, IFactory
{
    private FactoryQueueManager factoryQueueManager;
    public FactoryQueueManager FactoryQueueManager => factoryQueueManager;
    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent<FactoryQueueManager>(out factoryQueueManager))
        {
            Debug.LogError($"{nameof(factoryQueueManager)} is required for {GetType().Name} in gameobject {gameObject.name}!");
        }
    }
}
