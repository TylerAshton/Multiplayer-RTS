using Codice.Client.Common.GameUI;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace Editor.ProjectileEditor
{
    [CustomPropertyDrawer(typeof(BaseAbilityStat), true)]
    public class AbilityStatPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Dimensions and Rects
            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect buttonRect = new Rect(position.x + position.width - 75, position.y, 80, lineHeight);
            Rect fieldRect = new Rect(position.x, position.y, position.width - 80, lineHeight);
            Rect editButtonRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(buttonRect, "Create New"))
            {
                ShowNamingWindow(property);
                    
            }

            //Debug.Log(fieldInfo.FieldType);

            EditorGUI.EndProperty();
        }

        private void ShowNamingWindow(SerializedProperty property)
        {
            Type statType = fieldInfo.FieldType; // So this gets of the type of what this drawer field is

            UnityEditor.PopupWindow.Show(
                new Rect(new Vector2(100, 100), new Vector2(250, 100)),
                new AbilityStatNamePopup(CreateNewStatSO, property, statType) // TODO: Get the type from the property dynamically
            );
        }

        /// <summary>
        /// Creates a new ability of the specified type and assigns it to the property.
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="property"></param>
        private void CreateNewStatSO(string objectName, SerializedProperty property, Type statType)
        {
            // Calculating path for new ability asseet
            string folderPath = $"Assets/Resources/AbilityStats/{statType}"; // NOTE: This should be fine? Like we can't make static vars for the base class so this is the next best thing
            string assetName = $"{objectName}.asset";
            string fullPath = $"{folderPath}/{assetName}";

            // Create asset
            ScriptableObject newProjectile = ScriptableObject.CreateInstance(statType);


            // Save the asset to the specified path
            AssetDatabase.CreateAsset(newProjectile, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            property.objectReferenceValue = newProjectile;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
}
