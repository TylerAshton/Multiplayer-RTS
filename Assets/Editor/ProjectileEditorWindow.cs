using Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ProjectileEditorWindow : EditorWindow
    {
        private ProjectileStats selectedProjectile; // TODO: We could use a generic type here to allow editing of any ScriptableObject that has a DrawInspector method

        public static void Open(ProjectileStats _projectileStats)
        {
            ProjectileEditorWindow window = GetWindow<ProjectileEditorWindow>("Projectile Editor");
            window.selectedProjectile = _projectileStats;
            window.Show();
        }

        private void OnGUI()
        {
            if (selectedProjectile == null)
            {
                EditorGUILayout.LabelField("No ability selected.");
                return;
            }

            SerializedObject so = new SerializedObject(selectedProjectile);

            so.Update();

            // Draw ability fields
            selectedProjectile.DrawInspector(so);

            so.ApplyModifiedProperties();

        }
    }
}
