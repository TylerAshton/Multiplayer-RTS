using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class RegistryItem : ScriptableObject
{
    [SerializeField]
    [FormerlySerializedAs("abilityID")]
    [FormerlySerializedAs("iD")] protected string id;
    public string ID => id;

#if UNITY_EDITOR
    public virtual void DrawInspector(SerializedObject _so)
    {
        SerializedProperty fieldID = _so.FindProperty("id");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (fieldID.stringValue == "")
        {
            EditorGUILayout.HelpBox("ID Can't be null", MessageType.Error);
        }
    }
#endif

}
