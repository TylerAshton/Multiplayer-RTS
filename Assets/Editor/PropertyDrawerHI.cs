using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(AbilityReference))]
public class PropertyDrawerHI : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var abilityProp = property.FindPropertyRelative("ability");

        // Show property field with label
        EditorGUI.PropertyField(position, abilityProp, label, true);
        EditorGUI.EndProperty();
    }

}
