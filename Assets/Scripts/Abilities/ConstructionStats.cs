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

    [SerializeField] private VfxObject spawnVfx;
    public VfxObject SpawnVfx => spawnVfx;
    [SerializeField] private VfxObject summonVfx;
    public VfxObject SummonVfx => summonVfx;
    
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

        BeaconUtility.DrawStat<VfxObject>(_so, "spawnVfx");

        BeaconUtility.DrawStat<VfxObject>(_so, "summonVfx");

        // Min max slider tom foolery
        SerializedProperty fieldMinDisperstion = _so.FindProperty("minDisperstion");
        SerializedProperty fieldMaxDispersion = _so.FindProperty("maxDispersion");

        float min = fieldMinDisperstion.floatValue; // yay lines are now readable
        float max = fieldMaxDispersion.floatValue;

        fieldMinDisperstion.floatValue = EditorGUILayout.Slider("Min Dispersion", min, 0f, max);
        fieldMaxDispersion.floatValue = EditorGUILayout.Slider("Max Dispersion", max, min, 20f);


        SerializedProperty fieldOffset = _so.FindProperty("offset");
        EditorGUILayout.PropertyField(fieldOffset, new GUIContent("Offset"));

        SerializedProperty fieldQueueIcon = _so.FindProperty("queueIcon");
        fieldQueueIcon.objectReferenceValue = EditorGUILayout.ObjectField("Queue Icon", fieldQueueIcon.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
        if (fieldQueueIcon.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Icon must be set!", MessageType.Error);
        }
    }
#endif
}
