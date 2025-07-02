using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channelled Projection Ability", menuName = "Abilities/Channelled Projection")]
public class ChannelledProjection : Ability<ICharacterAbilityUser>
{
    [SerializeField] private float slowAmount;
    [SerializeField] private ProjectionStats channelStats;
    protected override void ActivateTyped(ICharacterAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{AnimationTrigger}");

        StatModifyer statModifyer = new StatModifyer(StatType.MoveSpeed, -slowAmount);
        List<StatModifyer> statModifyers = new List<StatModifyer>();
        statModifyers.Add(statModifyer);

        Effect newEffect = new Effect(CastTime, statModifyers);

        _user.EffectManager.AddEffect(newEffect);
    }

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject newProjection = Instantiate(GetProjectionBlueprint(), castPositionTransform.position, Quaternion.identity);
        newProjection.GetComponent<NetworkObject>().Spawn();
        newProjection.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        /*ProjectionManager projectionManager = newProjection.GetComponent<ProjectionManager>();
        projectionManager.*/
        HitboxManager hitboxManager = newProjection.GetComponent<HitboxManager>();
        hitboxManager.ApplyHitboxStatsWithID(channelStats.HitboxStats.ID);
        //newProjection.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }

    private GameObject GetProjectionBlueprint()
    {
        GameObject projectile = Resources.Load<GameObject>("Blueprints/BPProjection");
        return projectile;
    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldSlowAmount = _so.FindProperty("slowAmount");
        EditorGUILayout.PropertyField(fieldSlowAmount);
        if (fieldSlowAmount.floatValue < 0)
        {
            EditorGUILayout.HelpBox("Slow amount must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldChannelStats = _so.FindProperty("channelStats");
        EditorGUILayout.PropertyField(fieldChannelStats);

        if (fieldChannelStats.objectReferenceValue != null)
        {
            DrawStat(fieldChannelStats);
        }
    }
#endif
}
