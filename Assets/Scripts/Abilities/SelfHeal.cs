using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Self-Heal Ability", menuName = "Abilities/Self-Heal")]
public class SelfHeal : Ability<IUnitAbilityUser>
{
    [SerializeField] private float healAmount = 50f;
    protected override void OnCastTyped(IUnitAbilityUser _user)
    {
        _user.Health.Heal(healAmount);
    }

    protected override void DebugDrawingTyped(IUnitAbilityUser _user)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnApexTyped(IUnitAbilityUser _user)
    {
        throw new System.NotImplementedException();
    }

#if UNITY_EDITOR // Will crash if this is not wrapped in UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty healAmountProperty = _so.FindProperty("healAmount");
        EditorGUILayout.PropertyField(healAmountProperty, new GUIContent("Heal Amount", "The amount of health to restore when this ability is used."));
        if (healAmountProperty.floatValue < 0)
        {
            EditorGUILayout.HelpBox("Heal Amount cannot be negative.", MessageType.Error);
        }


    }
#endif
}
