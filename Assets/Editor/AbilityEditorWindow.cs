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

            // Custom fields
            SerializedProperty fieldID = so.FindProperty("abilityID");
            fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
            if (fieldID.stringValue == "")
            {
                EditorGUILayout.HelpBox("Ability ID Can't be null", MessageType.Error);
            }

            SerializedProperty fieldName = so.FindProperty("abilityName");
            fieldName.stringValue = EditorGUILayout.TextField("Name", fieldName.stringValue);

            SerializedProperty fieldAnimationTrigger = so.FindProperty("animationTrigger");
            fieldAnimationTrigger.stringValue = EditorGUILayout.TextField("Animation Trigger", fieldAnimationTrigger.stringValue); // TODO: Remove?
            //EditorGUILayout.HelpBox("Honestly don't animationTrigger touch this without a dev.", MessageType.Warning);

            SerializedProperty fieldCastTime = so.FindProperty("castTime");
            fieldCastTime.floatValue = EditorGUILayout.Slider("Cast Time", fieldCastTime.floatValue, 0, 10);

            SerializedProperty fieldCooldown = so.FindProperty("cooldown");
            fieldCooldown.floatValue = EditorGUILayout.Slider("Cooldown", fieldCooldown.floatValue, 0, 60);

            SerializedProperty fieldAbilityCost = so.FindProperty("abilityCost");
            fieldAbilityCost.intValue = EditorGUILayout.IntField("Ability Cost", fieldAbilityCost.intValue);

            if (fieldAbilityCost.intValue < 0)
            {
                EditorGUILayout.HelpBox("Ability Cost cannot be negative.", MessageType.Error);
            }
            


            SerializedProperty fieldCastPos = so.FindProperty("castPositionName");
            fieldCastPos.enumValueIndex = EditorGUILayout.Popup("Cast Position", fieldCastPos.enumValueIndex, fieldCastPos.enumDisplayNames);

            SerializedProperty fieldIcon = so.FindProperty("icon");
            fieldIcon.objectReferenceValue = EditorGUILayout.ObjectField("Ability Icon", fieldIcon.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);

            so.ApplyModifiedProperties();

        }
    }
}

