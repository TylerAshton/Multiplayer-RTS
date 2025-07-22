using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Ability", menuName = "Abilities/Buff")]
public class BuffAbility : Ability<ICharacterAbilityUser>
{
    [SerializeField] private GameObject buffEffects;
    [SerializeField] private float slowAmount;
    [SerializeField] private Effect effect;
    protected override string animationTrigger => "BuffAbility";
/*    public override Ability Clone()
    {
        throw new System.NotImplementedException();
    }*/

    protected override void ActivateTyped(ICharacterAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{animationTrigger}");
        StatModifyer statModifyer = new StatModifyer(StatType.MoveSpeed, -slowAmount);
        List<StatModifyer> statModifyers = new List<StatModifyer>();
        statModifyers.Add(statModifyer);

        Effect newEffect = new Effect(CastTime, statModifyers); 
        
        _user.EffectManager.AddEffect(newEffect);
    }

/*    protected override void CopySubclassTo(Ability _target)
    {
        base.CopyBaseTo(_target);
        _target.buffEffects = this.buffEffects;
    }*/

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {

    }

    protected override void OnUseTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject buffVfx = Instantiate(buffEffects, _user.Transform);
        buffVfx.GetComponent<NetworkObject>().Spawn();
        buffVfx.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        _user.EffectManager.AddEffect(effect);
    }
#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldBuffEffects = _so.FindProperty("buffEffects");
        fieldBuffEffects.objectReferenceValue = EditorGUILayout.ObjectField("Buff Effects Prefab", fieldBuffEffects.objectReferenceValue, typeof(GameObject), false);
        if (fieldBuffEffects.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Buff Effects Prefab must be assigned.", MessageType.Error);
        }


        SerializedProperty fieldEffect = _so.FindProperty("effect");
        EditorGUILayout.PropertyField(fieldEffect, true);
    }
#endif
}
