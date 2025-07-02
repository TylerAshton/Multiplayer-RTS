using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channel Stats", menuName = "Stats/Channel Stats")]
public class ProjectionStats : BaseAbilityStat
{
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool damageOnce = false;
    [SerializeField] private float duration = 5f;
    [SerializeField] private HitboxStats hitboxStats;

    // Custom HitBox manager
    public GameObject VFXPrefab => vfxPrefab;
    public Vector3 VFXOffset => vfxOffset;
    public bool DamageOnce => damageOnce;
    public float Damage => damage;
    public float Duration => duration;
    public HitboxStats HitboxStats => hitboxStats;



    public override bool IsValid()
    {
        if (base.IsValid() == false)
        {
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
        if (damage <= 0)
        {
            Debug.LogError($"{name} Damage is zero or negative: {damage}");
            return false;
        }
        if (hitboxStats == null)
        {
            Debug.LogError($"{name} HitboxStats is not assigned.");
            return false;
        }

        return true;
    }

    public override void DrawInspector(SerializedObject so)
    {
        base.DrawInspector(so);

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

        SerializedProperty fieldDamagePerSecond = so.FindProperty("damage");
        fieldDamagePerSecond.floatValue = EditorGUILayout.FloatField("Damage", fieldDamagePerSecond.floatValue);
        if (fieldDamagePerSecond.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Damage must be greater than 0!", MessageType.Error);
        }

        SerializedProperty serializedProperty = so.FindProperty("damageOnce");
        serializedProperty.boolValue = EditorGUILayout.Toggle("Damage Once", serializedProperty.boolValue);

        SerializedProperty fieldDuration = so.FindProperty("duration");
        fieldDuration.floatValue = EditorGUILayout.FloatField("Duration", fieldDuration.floatValue);
        if (fieldDuration.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Duration must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldHitbox = so.FindProperty("hitboxStats");
        EditorGUILayout.PropertyField(fieldHitbox);

        if (fieldHitbox.objectReferenceValue != null)
        {
            DrawStat(fieldHitbox);
            
        }
    }

#if UNITY_EDITOR
    protected void DrawStat(SerializedProperty _sp)
    {
        if (_sp.objectReferenceValue == null)
        {
            Debug.LogError($"SerializedProperty is null in {GetType().Name}. Please assign a valid SerializedProperty.");
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
