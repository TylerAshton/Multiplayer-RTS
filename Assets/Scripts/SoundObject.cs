using FMODUnity;
using UnityEditor;
using UnityEngine;

public class SoundObject : RegistryItem, Inspectorable
{
    [SerializeField] private EventReference soundEvent;
    public EventReference SoundEvent => soundEvent;
    [SerializeField] private bool isPlayingLocal = true;
    public bool IsPlayingLocal => isPlayingLocal;
    [SerializeField] private EventReference soundEvent2D;
    public EventReference SoundEvent2D => soundEvent2D;



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

        SerializedProperty fieldLocalPlay = _so.FindProperty("isPlayingLocal");
        fieldLocalPlay.boolValue = EditorGUILayout.Toggle("Local play", fieldLocalPlay.boolValue);

        if (isPlayingLocal)
        {
            SerializedProperty fieldCastSound2D = _so.FindProperty("soundEvent2D");
            EditorGUILayout.PropertyField(fieldCastSound2D, new GUIContent("2D Sound Variant"));

            if (soundEvent2D.Guid.IsNull)
            {
                EditorGUILayout.HelpBox("If the original sound is 3D this will not play correctly, please insert a 2D variant above!", MessageType.Warning);
            }
        }
    }
#endif
}
