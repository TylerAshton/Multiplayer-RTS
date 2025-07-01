using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channelled Projection Ability", menuName = "Abilities/Channelled Projection")]
public class ChannelledProjection : Ability<ICharacterAbilityUser>
{
    [SerializeField] private float slowAmount;
    [SerializeField] private ChannelStats channelStats;
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
/*        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject newEffect = Instantiate(vfxPrefab, castPositionTransform);
        newEffect.GetComponent<NetworkObject>().Spawn();
        newEffect.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        newEffect.GetComponent<IFaction>().Faction = _user.IFaction.Faction;*/
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
