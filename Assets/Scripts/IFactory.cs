using UnityEngine;

public interface IFactory : IAbilityUser
{
    FactoryQueueManager FactoryQueueManager { get; }
}
