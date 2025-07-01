using UnityEditor;
using UnityEngine;

public abstract class BaseAbilityStat : ScriptableObject
{
    public abstract string folderName { get; } // Folder for the scriptable objects

    public abstract bool IsValid();

#if UNITY_EDITOR
    public abstract void DrawInspector(SerializedObject so);
#endif
}
