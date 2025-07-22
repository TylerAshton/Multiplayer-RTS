
using UnityEditor;
using UnityEngine;
public static class BeaconUtility
{
#if UNITY_EDITOR
    public static void DrawStat(SerializedProperty _sp)
    {
        if (_sp.objectReferenceValue == null)
        {
            Debug.LogError($"SerializedProperty is null. Please assign a valid SerializedProperty.");
        }

        SerializedObject statsSO = new SerializedObject(_sp.objectReferenceValue);
        BaseAbilityStat stat = (BaseAbilityStat)_sp.objectReferenceValue;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{stat.name}", EditorStyles.boldLabel);

        statsSO.Update();

        stat.DrawInspector(statsSO);

        statsSO.ApplyModifiedProperties();
    }
#endif

}

