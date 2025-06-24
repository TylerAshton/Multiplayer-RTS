using System;
using UnityEditor;
using UnityEngine;


namespace Editor.ProjectileEditor
{
    public class ProjectileNamePopup : PopupWindowContent
    {
        private string abilityName = "New Projectile";
        private System.Action<string, SerializedProperty> onConfirm;
        private SerializedProperty property;

        public ProjectileNamePopup(System.Action<string, SerializedProperty> confirmCallback, SerializedProperty _property)
        {
            onConfirm = confirmCallback;
            property = _property;
        }

        public override Vector2 GetWindowSize() => new Vector2(250, 60);

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);

            GUILayout.Label("Enter Projectile Name", EditorStyles.boldLabel);
            abilityName = EditorGUILayout.TextField(abilityName);

            if (GUILayout.Button("Create"))
            {
                onConfirm?.Invoke(abilityName, property);
                editorWindow.Close();
            }

            GUILayout.EndArea();
        }
    }
}
