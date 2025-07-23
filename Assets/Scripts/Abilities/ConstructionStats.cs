using Cinemachine.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ConstructionStats : BaseAbilityStat
{
    private const float minVfxSize = 0.001f;
    private const float maxVfxSize = 20f;
    [SerializeField] private float constructionTime = 5f;
    public float ConstructionTime => constructionTime;

    [SerializeField] private GameObject consutrctablePrefab;
    public GameObject ConstructablePrefab => consutrctablePrefab;


    [SerializeField] private GameObject spawnVFX;
    public GameObject SpawnVFX => spawnVFX;
    [SerializeField] private float spawnVFXScale = 1f;
    public float SpawnVFXScale => spawnVFXScale;

    [SerializeField] private GameObject summonVFX;
    public GameObject SummonVFX => summonVFX;
    [SerializeField] private float summonVFXScale = 1f;
    public float SummonVFXScale => summonVFXScale;
    public float VfxDespawnTime => 5f;
    [SerializeField] private float maxDispersion = 5f;
    public float MaxDispersion => maxDispersion;
    [SerializeField] private float minDisperstion = 5f;
    public float MinDisperstion => minDisperstion;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [SerializeField] private Sprite queueIcon;
    public Sprite QueueIcon => queueIcon;

    public Vector3 Offset => offset;

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldConstructionTime = _so.FindProperty("constructionTime");
        fieldConstructionTime.floatValue = EditorGUILayout.FloatField("Construction Time", fieldConstructionTime.floatValue);
        if (fieldConstructionTime.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Construction Time must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldSpawnee = _so.FindProperty("consutrctablePrefab");
        fieldSpawnee.objectReferenceValue = EditorGUILayout.ObjectField("ConsutrctablePrefab", fieldSpawnee.objectReferenceValue, typeof(GameObject), false);
        if (fieldSpawnee.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("ConsutrctablePrefab must be assigned.", MessageType.Error);
        }

        SerializedProperty fieldSpawnVFX = _so.FindProperty("spawnVFX");
        fieldSpawnVFX.objectReferenceValue = EditorGUILayout.ObjectField("Spawn VFX Prefab", fieldSpawnVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldSpawnVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Spawn VFX Prefab must be assigned.", MessageType.Error);
        }
        SerializedProperty fieldSpawnVfxScale = _so.FindProperty("summonVFXScale");
        fieldSpawnVfxScale.floatValue = EditorGUILayout.Slider("Portal VFX Scale", fieldSpawnVfxScale.floatValue, minVfxSize, maxVfxSize);

        SerializedProperty fieldSummonVFX = _so.FindProperty("summonVFX");
        fieldSummonVFX.objectReferenceValue = EditorGUILayout.ObjectField("Portal VFX Prefab", fieldSummonVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldSummonVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Portal VFX Prefab must be assigned.", MessageType.Error);
        }

        SerializedProperty fieldSummonVfxScale = _so.FindProperty("spawnVFXScale");
        fieldSummonVfxScale.floatValue = EditorGUILayout.Slider("Spawn VFX Scale", fieldSummonVfxScale.floatValue, minVfxSize, maxVfxSize);

        // Min max slider tom foolery
        SerializedProperty fieldMinDisperstion = _so.FindProperty("minDisperstion");
        SerializedProperty fieldMaxDispersion = _so.FindProperty("maxDispersion");

        float min = fieldMinDisperstion.floatValue; // yay lines are now readable
        float max = fieldMaxDispersion.floatValue;

        fieldMinDisperstion.floatValue = EditorGUILayout.Slider("Min Dispersion", min, 0f, max);
        fieldMaxDispersion.floatValue = EditorGUILayout.Slider("Max Dispersion", max, min, 20f);


        SerializedProperty fieldOffset = _so.FindProperty("offset");
        EditorGUILayout.PropertyField(fieldOffset, new GUIContent("Offset"));
    }
#endif
}
