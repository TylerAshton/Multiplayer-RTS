using FMODUnity;
using UnityEditor;
using UnityEngine;

public class SoundObject : RegistryItem, Inspectorable
{
    [SerializeField] private EventReference soundEvent;
    public EventReference SoundEvent => soundEvent;



#if UNITY_EDITOR 
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldCastSound = _so.FindProperty("soundEvent");
        EditorGUILayout.PropertyField(fieldCastSound, new GUIContent("Sound Event"));

        if (soundEvent.Guid.IsNull)
        {
            EditorGUILayout.HelpBox("Sound Event cannot be null.", MessageType.Error);
        }
    }
#endif
}
