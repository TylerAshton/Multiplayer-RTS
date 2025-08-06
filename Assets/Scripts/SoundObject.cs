using FMODUnity;
using UnityEditor;
using UnityEngine;

public class SoundObject : RegistryItem, Inspectorable
{
    [SerializeField] private EventReference soundEvent;
    public EventReference SoundEvent => soundEvent;
    [SerializeField] private bool has2DVariant = true;
    public bool Has2DVariant => has2DVariant;
    [SerializeField] private bool onlyPlayForCaster = false;
    public bool OnlyPlayForCaster => onlyPlayForCaster;
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

        SerializedProperty fieldOnlyPlayForCaster = _so.FindProperty("onlyPlayForCaster");
        fieldOnlyPlayForCaster.boolValue = EditorGUILayout.Toggle("Only play for caster", fieldOnlyPlayForCaster.boolValue);

        if (onlyPlayForCaster)
        {
            EditorGUILayout.HelpBox("This sound will only play for the caster, not for other players.", MessageType.Info);
            return;
        }

        SerializedProperty fieldLocalPlay = _so.FindProperty("has2DVariant");
        fieldLocalPlay.boolValue = EditorGUILayout.Toggle("Play 2D variant for caster", fieldLocalPlay.boolValue);

        if (has2DVariant)
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
