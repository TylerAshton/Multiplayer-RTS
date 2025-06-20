using UnityEditor;
using UnityEngine;


namespace Editor
{
    [CustomPropertyDrawer(typeof(ProjectileStats))]
    public class ProjectileDrawer : PropertyDrawer
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

            /*EditorGUI.LabelField(editButtonRect, "hi");*/

            if (property.objectReferenceValue != null)
            {
                if (GUI.Button(editButtonRect, "Edit"))
                {
                    ProjectileEditorWindow.Open((ProjectileStats)property.objectReferenceValue);
                }
            }


            EditorGUI.EndProperty();
        }

        private void ShowNamingWindow(SerializedProperty property)
        {
            UnityEditor.PopupWindow.Show(
                new Rect(new Vector2(100, 100), new Vector2(250, 100)),
                new ProjectileNamePopup(CreateNewProjectile, property)
            );
        }

        /// <summary>
        /// Creates a new ability of the specified type and assigns it to the property.
        /// </summary>
        /// <param name="projectileName"></param>
        /// <param name="property"></param>
        private void CreateNewProjectile(string projectileName, SerializedProperty property)
        {
            // Calculating path for new ability asseet
            string folderPath = "Assets/Resources/Projectiles";
            string assetName = $"{projectileName}.asset";
            string fullPath = $"{folderPath}/{assetName}";

            // Create asset
            ScriptableObject newProjectile = ScriptableObject.CreateInstance<ProjectileStats>();

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
