using UnityEditor;
using UnityEngine;

public abstract class BaseAbilityStat : ScriptableObject
{
    public abstract bool IsValid();

#if UNITY_EDITOR
    public abstract void DrawInspector(SerializedObject so);
#endif
}
