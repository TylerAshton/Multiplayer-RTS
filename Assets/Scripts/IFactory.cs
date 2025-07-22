using UnityEngine;

public interface IFactory : IUnitAbilityUser
{
    FactoryQueueManager FactoryQueueManager { get; }
}
