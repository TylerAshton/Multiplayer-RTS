using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channel Stats", menuName = "Stats/Channel Stats")]
public class ProjectionStats : BaseAbilityStat
{
    [SerializeField] private string iD;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private Vector3 hitboxOffset = Vector3.zero;
    // Custom HitBox manager

    public string ID => iD;
    public GameObject VFXPrefab => vfxPrefab;
    public Vector3 VFXOffset => vfxOffset;
    public float DamagePerSecond => damagePerSecond;
    public float Duration => duration;
    public Vector3 HitboxOffset => hitboxOffset;

    public override string folderName => "Projections";

    public override bool IsValid()
    {
        if (this.ID == null || this.ID.Trim().Length == 0)
        {
            Debug.LogError($"{name} has no ID assigned or ID is empty.");
            return false;
        }
        if (vfxPrefab == null)
        {
            Debug.LogError($"{name} VFXPrefab is not assigned.");
            return false;
        }
        if (duration <= 0)
        {
            Debug.LogError($"{name} Duration is zero or negative: {duration}");
            return false;
        }
        if (damagePerSecond <= 0)
        {
            Debug.LogError($"{name} DamagePerSecond is zero or negative: {damagePerSecond}");
            return false;
        }
        if (hitboxOffset == null)
        {
            Debug.LogError($"{name} HitboxOffset is not assigned.");
            return false;
        }

        return true;
    }

    public override void DrawInspector(SerializedObject so)
    {
        SerializedProperty fieldID = so.FindProperty("iD");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (string.IsNullOrWhiteSpace(fieldID.stringValue))
        {
            EditorGUILayout.HelpBox("ID must be assigned and cannot be empty!", MessageType.Error);
        }

        SerializedProperty fieldVFXPrefab = so.FindProperty("vfxPrefab");
        fieldVFXPrefab.objectReferenceValue = EditorGUILayout.ObjectField("VFX Prefab", fieldVFXPrefab.objectReferenceValue, typeof(GameObject), false);
        if (fieldVFXPrefab.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("VFX Prefab must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldVFXOffset = so.FindProperty("vfxOffset");
        fieldVFXOffset.vector3Value = EditorGUILayout.Vector3Field("VFX Offset", fieldVFXOffset.vector3Value);
        if (fieldVFXOffset.vector3Value == null)
        {
            EditorGUILayout.HelpBox("VFX Offset must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldDamagePerSecond = so.FindProperty("damagePerSecond");
        fieldDamagePerSecond.floatValue = EditorGUILayout.FloatField("Damage Per Second", fieldDamagePerSecond.floatValue);
        if (fieldDamagePerSecond.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Damage Per Second must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldDuration = so.FindProperty("duration");
        fieldDuration.floatValue = EditorGUILayout.FloatField("Duration", fieldDuration.floatValue);
        if (fieldDuration.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Duration must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldHitboxOffset = so.FindProperty("hitboxOffset");
        fieldHitboxOffset.vector3Value = EditorGUILayout.Vector3Field("Hitbox Offset", fieldHitboxOffset.vector3Value);
        if (fieldHitboxOffset.vector3Value == null)
        {
            EditorGUILayout.HelpBox("Hitbox Offset must be assigned!", MessageType.Error);
        }
    }

}
