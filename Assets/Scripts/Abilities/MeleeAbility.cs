using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee")]
public class MeleeAbility : Ability<IAbilityUser>
{
    [SerializeField] private float angleDegrees = 90f;
    [SerializeField] private float range = 4f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private GameObject hitEffect;

    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        Gizmos.color = Color.yellow;
        Vector3 forward = _user.Transform.forward;

        Vector3 leftSide = Quaternion.AngleAxis(angleDegrees / 2, Vector3.up) * forward;
        leftSide = _user.Transform.position + leftSide * range;
        Vector3 rightSide = Quaternion.AngleAxis(-angleDegrees / 2, Vector3.up) * forward;
        rightSide = _user.Transform.position + rightSide * range;


        Gizmos.DrawLine(_user.Transform.position, rightSide);
        Gizmos.DrawLine(_user.Transform.position, leftSide);
    }

    /// <summary>
    /// Function called when the animation reaches the peak of its swing
    /// </summary>
    /// <param name="_user"></param>
    protected override void OnUseTyped(IAbilityUser _user)
    {
        Vector3 origin = _user.Transform.position;
        Vector3 forward = _user.Transform.forward;
        float cosAngle = Mathf.Cos(angleDegrees * 0.5f * Mathf.Deg2Rad); // Conversion of our angleDegrees to a cos for dot. 
                                                                         // We use half as the full cone is angleDegrees

        Collider[] hits = Physics.OverlapSphere(origin, range);
        foreach (var hit in hits)
        {
            // Skip if the hit object is part of the same faction
            if (hit.TryGetComponent<IFaction>(out IFaction faction))
            {
                if (faction.Faction == _user.IFaction.Faction)
                {
                    continue;
                }
            }

            Vector3 toTarget = (hit.transform.position - origin).normalized;
            float dot = Vector3.Dot(forward, toTarget);

            if (dot >= cosAngle) // If is within our cone degrees, hit
            {
                if (hit.TryGetComponent(out Health _health))
                {
                    _health.Damage(damage);
                    GameObject hitVFX = Instantiate(hitEffect, hit.transform);
                    hitVFX.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }

    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldAngleDegrees = _so.FindProperty("angleDegrees");
        fieldAngleDegrees.floatValue = EditorGUILayout.Slider("Angle Degrees", fieldAngleDegrees.floatValue, 0, 360);

        SerializedProperty fieldRange = _so.FindProperty("range");
        fieldRange.floatValue = EditorGUILayout.Slider("Range", fieldRange.floatValue, 0, 10);

        SerializedProperty fieldDamage = _so.FindProperty("damage");
        fieldDamage.floatValue = EditorGUILayout.FloatField("Damage", fieldDamage.floatValue);
        if (fieldDamage.floatValue < 0)
        {
            EditorGUILayout.HelpBox("Ability Cost cannot be negative.", MessageType.Error);
        }

        SerializedProperty fieldHitEffect = _so.FindProperty("hitEffect");
        fieldHitEffect.objectReferenceValue = EditorGUILayout.ObjectField("Hit Effect Prefab", fieldHitEffect.objectReferenceValue, typeof(GameObject), false);
    }
}