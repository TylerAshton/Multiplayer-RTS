using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enqeuue Ability", menuName = "Abilities/EnqueueConstruction")]
public class EnqueueConstruction : Ability<IFactory>
{
    [SerializeField] private ConstructionStats constructionStats;
    protected override void OnCastTyped(IFactory _user)
    {
        _user.FactoryQueueManager.EnqueueUnit(constructionStats);
    }

    protected override void DebugDrawingTyped(IFactory _user)
    {

    }

    protected override void OnApexTyped(IFactory _user)
    {

    }

#if UNITY_EDITOR // Will crash if this is not wrapped in UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);


        BeaconUtility.DrawStat<BaseAbilityStat>(_so, "constructionStats", false);


    }
#endif
}
