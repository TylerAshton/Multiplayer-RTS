using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Stats", menuName = "Stats/Projectile")]
public class ProjectileStats : ScriptableObject
{
    [SerializeField] private string iD;
    public string ID => iD;
    // Consts for field sliers and validation
    private const int minAOERadius = 1;
    private const int maxAOERadius = 20;
    private const int minVFXRadius = 0;
    private const int maxVFXRadius = 10;
    [SerializeField] private float detectionRange = 0.1f;
    public float DetectionRange => detectionRange;
    [SerializeField] private float speed = 10f;
    public float Speed => speed;
    [SerializeField] private float damage = 1f;
    public float Damage => damage;
    [SerializeField] private float lifeTime = 5f;
    public float LifeTime => lifeTime;

    [SerializeField] private GameObject bulletVFX;
    public GameObject BulletVFX => bulletVFX;
    [SerializeField] float bulletVFXScale = 1f;
    public float BulletVFXScale => bulletVFXScale;

    [SerializeField] private GameObject deathVFX;
    public GameObject DeathVFX => deathVFX;
    [SerializeField] float deathVFXScale = 1f;
    public float DeathVFXScale => deathVFXScale;

    [SerializeField] private bool isAOE = false;
    public bool IsAOE => isAOE;
    [SerializeField] private float aoeRadius = 1f;
    public float AOERadius => aoeRadius;

    [SerializeField] private int penetration = 0;
    public int Penetration => penetration;



    /// <summary>
    /// Validation function for the projectile stats as it got way too long to be used in ApplyProjectileStats
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        if (this.ID == null || this.ID.Trim().Length == 0)
        {
            Debug.LogError($"{this.name} has no ID assigned or ID is empty.");
            return false;
        }

        if (this.DetectionRange <= 0)
        {
            Debug.LogError($"{this.name} DetectionRange is zero or negative: {this.DetectionRange}");
            return false;
        }

        if (this.Speed <= 0)
        {
            Debug.LogError($"{this.name} speed is zero or negative: {this.Speed}");
            return false;
        }

        if (this.Damage <= 0)
        {
            Debug.LogError($"{this.name} damage is zero or negative: {this.Damage}");
            return false;
        }

        if (this.LifeTime <= 0)
        {
            Debug.LogError($"{this.name} life time is zero or negative: {this.LifeTime}");
            return false;
        }

        if (this.BulletVFX == null)
        {
            Debug.LogError($"{this.name} has no BulletVFX assigned");
            return false;
        }

        if (this.bulletVFXScale <= 0)
        {
            Debug.LogError($"{this.name} has a zero or negative bullet vfx scale: {this.bulletVFXScale}");
            return false;
        }

        if (this.DeathVFX == null)
        {
            Debug.LogError($"{this.name} has no DeathVFX assigned.");
            return false;
        }

        if (this.deathVFXScale <= 0)
        {
            Debug.LogError($"{this.name} has a zero or negative death vfx scale: {this.deathVFXScale}");
            return false;
        }

        if (this.isAOE)
        {
            if (this.aoeRadius <= 0)
            {
                Debug.LogError($"{this.name} has a zero or negative AOE radius: {this.aoeRadius}");
                return false;
            }
        }

        if (this.penetration < 0)
        {
            Debug.LogError($"{this.name} has a negative penetration: {this.penetration}");
            return false;
        }


        return true;
    }
#if UNITY_EDITOR
    public void DrawInspector(SerializedObject so)
    {
        SerializedProperty fieldID = so.FindProperty("iD");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (string.IsNullOrWhiteSpace(fieldID.stringValue))
        {
            EditorGUILayout.HelpBox("ID must be assigned and cannot be empty!", MessageType.Error);
        }

        SerializedProperty fieldDetectionRange = so.FindProperty("detectionRange");
        fieldDetectionRange.floatValue = EditorGUILayout.FloatField("Detection Range", fieldDetectionRange.floatValue);
        if (fieldDetectionRange.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Detection Range must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldSpeed = so.FindProperty("speed");
        fieldSpeed.floatValue = EditorGUILayout.FloatField("Speed", fieldSpeed.floatValue);
        if (fieldSpeed.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Speed must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldDamage = so.FindProperty("damage");
        fieldDamage.floatValue = EditorGUILayout.FloatField("Damage", fieldDamage.floatValue);
        if (fieldDamage.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Damage must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldLifeTime = so.FindProperty("lifeTime");
        fieldLifeTime.floatValue = EditorGUILayout.FloatField("Life Time", fieldLifeTime.floatValue);
        if (fieldLifeTime.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Life Time must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldIsAOE = so.FindProperty("isAOE");
        fieldIsAOE.boolValue = EditorGUILayout.Toggle("Is AOE", fieldIsAOE.boolValue);
        if (fieldIsAOE.boolValue)
        {
            SerializedProperty fieldAOERadius = so.FindProperty("aoeRadius");
            fieldAOERadius.floatValue = EditorGUILayout.Slider("AOE Radius", fieldAOERadius.floatValue, minAOERadius, maxAOERadius);
            if (fieldAOERadius.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("AOE Radius must be greater than 0!", MessageType.Error);
            }
        }

        SerializedProperty fieldPenetration = so.FindProperty("penetration");
        fieldPenetration.intValue = EditorGUILayout.IntField("Penetration Amount", fieldPenetration.intValue);
        if (fieldPenetration.intValue < 0)
        {
            EditorGUILayout.HelpBox("Penetration Amount must be 0 or greater!", MessageType.Error);
        }

        SerializedProperty fieldBulletVFX = so.FindProperty("bulletVFX");
        fieldBulletVFX.objectReferenceValue = EditorGUILayout.ObjectField("Bullet VFX", fieldBulletVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldBulletVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Bullet VFX must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldBulletVFXScale = so.FindProperty("bulletVFXScale");
        fieldBulletVFXScale.floatValue = EditorGUILayout.Slider("Bullet VFX Scale", fieldBulletVFXScale.floatValue, minVFXRadius, maxVFXRadius);

        SerializedProperty fieldDeathVFX = so.FindProperty("deathVFX");
        fieldDeathVFX.objectReferenceValue = EditorGUILayout.ObjectField("Death VFX", fieldDeathVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldDeathVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Bullet VFX must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldDeathVFXScale = so.FindProperty("deathVFXScale");
        fieldDeathVFXScale.floatValue = EditorGUILayout.Slider("Death VFX Scale", fieldDeathVFXScale.floatValue, minVFXRadius, maxVFXRadius);
        EditorGUILayout.HelpBox("VFX Scaling works on some effects better than others due to how some authors make VFX.", MessageType.Warning);
    }
#endif
}
