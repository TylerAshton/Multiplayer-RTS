using UnityEngine;

[CreateAssetMenu(fileName = "New Enqeuue Ability", menuName = "Abilities/EnqueueConstruction")]
public class EnqueueConstruction : Ability<IFactory>
{
    [SerializeField] private ConstructionStats constructionStats;
    protected override void ActivateTyped(IFactory _user)
    {
        _user.FactoryQueueManager.EnqueueUnit(constructionStats);
    }

    protected override void DebugDrawingTyped(IFactory _user)
    {

    }

    protected override void OnUseTyped(IFactory _user)
    {

    }
}
