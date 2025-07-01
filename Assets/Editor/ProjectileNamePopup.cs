using System;
using UnityEditor;
using UnityEngine;


namespace Editor.ProjectileEditor
{
    public class ProjectileNamePopup : PopupWindowContent
    {
        private string abilityName = "New Projectile";
        private System.Action<string, SerializedProperty, Type> onConfirm;
        private SerializedProperty property;
        private Type type;

        public ProjectileNamePopup(System.Action<string, SerializedProperty, Type> confirmCallback, SerializedProperty _property, Type _type)
        {
            onConfirm = confirmCallback;
            property = _property;
            type = _type;

        }

        public override Vector2 GetWindowSize() => new Vector2(250, 60);

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);

            GUILayout.Label("Enter Projectile Name", EditorStyles.boldLabel);
            abilityName = EditorGUILayout.TextField(abilityName);

            if (GUILayout.Button("Create"))
            {
                onConfirm?.Invoke(abilityName, property, type);
                editorWindow.Close();
            }

            GUILayout.EndArea();
        }
    }
}
