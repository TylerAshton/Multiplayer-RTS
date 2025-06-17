using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Ability", menuName = "Abilities/Buff")]
public class BuffAbility : Ability<IAbilityUser>
{
    [SerializeField] private GameObject buffEffects;
    [SerializeField] private Effect effect;

/*    public override Ability Clone()
    {
        throw new System.NotImplementedException();
    }*/

    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{AnimationTrigger}");
    }

/*    protected override void CopySubclassTo(Ability _target)
    {
        base.CopyBaseTo(_target);
        _target.buffEffects = this.buffEffects;
    }*/

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {

    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject buffVfx = Instantiate(buffEffects, _user.Transform);
        buffVfx.GetComponent<NetworkObject>().Spawn();
        buffVfx.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        _user.EffectManager.AddEffect(effect);
    }

    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldBuffEffects = _so.FindProperty("buffEffects");
        fieldBuffEffects.objectReferenceValue = EditorGUILayout.ObjectField("Buff Effects Prefab", fieldBuffEffects.objectReferenceValue, typeof(GameObject), false);
        
        SerializedProperty fieldEffect = _so.FindProperty("effect");
        fieldEffect.objectReferenceValue = EditorGUILayout.ObjectField("Effect", fieldEffect.objectReferenceValue, typeof(Effect), false);
    }
}
