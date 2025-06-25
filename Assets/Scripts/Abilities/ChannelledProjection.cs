using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channelled Projection Ability", menuName = "Abilities/Channelled Projection")]
public class ChannelledProjection : Ability<ICharacterAbilityUser>
{
    [SerializeField] GameObject effect;
    protected override void ActivateTyped(ICharacterAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject newEffect = Instantiate(effect, castPositionTransform);
        newEffect.GetComponent<NetworkObject>().Spawn();
        newEffect.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        newEffect.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldEffect = _so.FindProperty("effect");
        fieldEffect.objectReferenceValue = EditorGUILayout.ObjectField("Effect Prefab", fieldEffect.objectReferenceValue, typeof(GameObject), false);
    }
#endif
}
