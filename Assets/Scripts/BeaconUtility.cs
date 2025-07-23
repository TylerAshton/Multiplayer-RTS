#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BeaconUtility
{
    public static void DrawStat<T>(SerializedObject _so, string _fieldName) where T : UnityEngine.Object, Inspectorable
    {
        SerializedProperty fieldBaseAbilityStat = _so.FindProperty(_fieldName);

        if (fieldBaseAbilityStat == null)
        {
            Debug.LogError($"SerializedProperty is null in {nameof(BeaconUtility)}. Please assign a valid SerializedProperty.");
            return;
        }

        EditorGUILayout.PropertyField(fieldBaseAbilityStat);

        if (fieldBaseAbilityStat.objectReferenceValue != null)
        {
            DrawStatValues<T>(fieldBaseAbilityStat);
        }
        else
        {
            EditorGUILayout.HelpBox($"Stats field cannot be null!", MessageType.Error);
        }

    }

    public static void DrawStatValues<T>(SerializedProperty _sp) where T : UnityEngine.Object, Inspectorable
    {
        if (_sp.objectReferenceValue == null)
        {
            Debug.LogError($"SerializedProperty is null in {nameof(BeaconUtility)}. Please assign a valid SerializedProperty.");
        }

        SerializedObject statsSO = new SerializedObject(_sp.objectReferenceValue);
        T stat = _sp.objectReferenceValue as T;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{nameof(T)}", EditorStyles.boldLabel);

        statsSO.Update();

        stat.DrawInspector(statsSO);

        statsSO.ApplyModifiedProperties();
    }
}
#endif
