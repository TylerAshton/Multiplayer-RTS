using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New VfxObject", menuName = "VfxObject")]
public class VfxObject : RegistryItem, Inspectorable
{
    [SerializeField] private GameObject vfxPrefab;
    public GameObject VfxPrefab => vfxPrefab;
    [SerializeField] private float vfxScale = 1f;
    public float VfxScale => vfxScale;
    [SerializeField] private float lingerTime = 5f;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    public Vector3 VfxOffset => vfxOffset;

    [SerializeField] private bool onlyShowForCaster = false;
    public bool OnlyShowForCaster => onlyShowForCaster;
    public float LingerTime => lingerTime;
    private const float minVfxSize = 0.001f;
    private const float maxVfxSize = 20;

    private const float minLingerTime = 0.001f;
    private const float maxLingerTime = 20;

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldvfxPrefab = _so.FindProperty("vfxPrefab");
        fieldvfxPrefab.objectReferenceValue = EditorGUILayout.ObjectField("VFX Prefab", fieldvfxPrefab.objectReferenceValue, typeof(GameObject), false);
        if (fieldvfxPrefab.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Spawn VFX Prefab must be assigned.", MessageType.Error);
        }
        SerializedProperty fieldVfxScale = _so.FindProperty("vfxScale");
        fieldVfxScale.floatValue = EditorGUILayout.Slider("VFX Scale", fieldVfxScale.floatValue, minVfxSize, maxVfxSize);

        SerializedProperty fieldLingerTime = _so.FindProperty("lingerTime");
        fieldLingerTime.floatValue = EditorGUILayout.Slider("VFX Duration", fieldLingerTime.floatValue, minLingerTime, maxLingerTime);

        SerializedProperty fieldVFXOffset = _so.FindProperty("vfxOffset");
        fieldVFXOffset.vector3Value = EditorGUILayout.Vector3Field("VFX Offset", fieldVFXOffset.vector3Value);
        if (fieldVFXOffset.vector3Value == null)
        {
            EditorGUILayout.HelpBox("VFX Offset must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldOnlyShowForCaster = _so.FindProperty("onlyShowForCaster");
        fieldOnlyShowForCaster.boolValue = EditorGUILayout.Toggle("Only Show for caster", fieldOnlyShowForCaster.boolValue);
    }
#endif
}
