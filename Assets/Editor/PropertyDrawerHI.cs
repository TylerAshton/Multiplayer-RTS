using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomPropertyDrawer(typeof(Ability))]
    public class PropertyDrawerHI : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect fieldRect = new Rect(position.x, position.y, position.width - 80, lineHeight);
            Rect buttonRect = new Rect(position.x + position.width - 75, position.y, 75, lineHeight);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(buttonRect, "Create New"))
            {
                GenericMenu menu = new GenericMenu();

                // Add ability types here - replace with your own subclasses
                menu.AddItem(new GUIContent("AOE Ability"), false, () => CreateNewAbilityOfType($"{typeof(AOEAbility).FullName}", property));
                menu.AddItem(new GUIContent("Buff Ability"), false, () => CreateNewAbilityOfType($"{typeof(BuffAbility).FullName}", property));
                menu.AddItem(new GUIContent("Construct Ability"), false, () => CreateNewAbilityOfType($"{typeof(Construct).FullName}", property));
                menu.AddItem(new GUIContent("Melee Ability"), false, () => CreateNewAbilityOfType($"{typeof(MeleeAbility).FullName}", property));
                menu.AddItem(new GUIContent("Projectile Ability"), false, () => CreateNewAbilityOfType($"{typeof(ProjectileAbility).FullName}", property));
                menu.AddItem(new GUIContent("Summon Ability"), false, () => CreateNewAbilityOfType($"{typeof(SummonAbility).FullName}", property));
                menu.AddItem(new GUIContent("DEBUG Ability"), false, () => CreateNewAbilityOfType($"{typeof(DebugAbility).FullName}", property));

                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }

        private void CreateNewAbilityOfType(string abilityTypeName, SerializedProperty property)
        {
            // Calculating path for new ability asseet
            string folderPath = "Assets/Resources/Abilities";
            string assetName = $"New {abilityTypeName}.asset";
            string fullPath = $"{folderPath}/{assetName}";

            // Create instance by type name
            var type = GetAbilityType(abilityTypeName);

            if (type == null)
            {
                Debug.LogError("abilityTypeName wasn't valid");
                return;
            }

            // Create asset

            ScriptableObject newAbility = ScriptableObject.CreateInstance(type);

            // Save the asset to the specified path
            AssetDatabase.CreateAsset(newAbility, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            property.objectReferenceValue = newAbility;
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Returns the type of the ability based on the type name provided.
        /// </summary>
        /// <param name="_typeName"></param>
        /// <returns></returns>
        private Type GetAbilityType(string _typeName) // TODO: wtf is this, is there not a better way to do this?
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .FirstOrDefault(t => t.Name == _typeName && typeof(Ability).IsAssignableFrom(t));
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }



    }
}



