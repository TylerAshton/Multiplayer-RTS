using System;
using UnityEditor;
using UnityEngine;

public class AbilityNamePopup : PopupWindowContent
{
    private string abilityName = "New Ability";
    private System.Action<string, Type, SerializedProperty> onConfirm;
    private Type abilityType;
    private SerializedProperty property;

    public AbilityNamePopup(System.Action<string, Type, SerializedProperty> confirmCallback, Type _abilityType, SerializedProperty _property)
    {
        onConfirm = confirmCallback;
        abilityType = _abilityType;
        property = _property;
    }

    public override Vector2 GetWindowSize() => new Vector2(250, 60);

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Enter Ability Name", EditorStyles.boldLabel);
        abilityName = EditorGUILayout.TextField(abilityName);

        if (GUILayout.Button("Create"))
        {
            onConfirm?.Invoke(abilityName, abilityType, property);
            editorWindow.Close();
        }
    }
}
