using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AbilityEditorWindow : EditorWindow
    {
        private Ability selectedAbility;

        public static void Open(Ability _ability)
        {
            AbilityEditorWindow window = GetWindow<AbilityEditorWindow>("Ability Editor");
            window.selectedAbility = _ability;
            window.Show();
        }

        private void OnGUI()
        {
            if (selectedAbility == null)
            {
                EditorGUILayout.LabelField("No ability selected.");
                return;
            }

            SerializedObject so = new SerializedObject(selectedAbility);

            so.Update();

            // Draw ability fields
            selectedAbility.DrawInspector(so);
            
            so.ApplyModifiedProperties();

        }
    }
}

