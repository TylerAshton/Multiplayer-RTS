using UnityEditor;
using UnityEngine;

public abstract class BaseAbilityStat : RegistryItem
{
    public virtual bool IsValid()
    {
        if (this.ID == null || this.ID.Trim().Length == 0) // Use this instead of string.IsNullOrEmpty as it also checks for whitespace
        {
            Debug.LogError($"{this.name} has no ID assigned or ID is empty.");
            return false;
        }
        return true;
    }

#if UNITY_EDITOR
    public virtual void DrawInspector(SerializedObject so)
    {
        SerializedProperty fieldID = so.FindProperty("iD");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (string.IsNullOrWhiteSpace(fieldID.stringValue))
        {
            EditorGUILayout.HelpBox("ID must be assigned and cannot be empty!", MessageType.Error);
        }
    }
#endif
}
