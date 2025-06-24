using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Stats", menuName = "Stats/Projectile")]
public class ProjectileStats : ScriptableObject
{
    [SerializeField] private string iD;
    public string ID => iD;
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

        SerializedProperty fieldBulletVFX = so.FindProperty("bulletVFX");
        fieldBulletVFX.objectReferenceValue = EditorGUILayout.ObjectField("Bullet VFX", fieldBulletVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldBulletVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Bullet VFX must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldBulletVFXScale = so.FindProperty("bulletVFXScale");
        fieldBulletVFXScale.floatValue = EditorGUILayout.Slider("Bullet VFX Scale", fieldBulletVFXScale.floatValue, 0, 10);

        SerializedProperty fieldDeathVFX = so.FindProperty("deathVFX");
        fieldDeathVFX.objectReferenceValue = EditorGUILayout.ObjectField("Death VFX", fieldDeathVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldDeathVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Bullet VFX must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldDeathVFXScale = so.FindProperty("deathVFXScale");
        fieldDeathVFXScale.floatValue = EditorGUILayout.Slider("Death VFX Scale", fieldDeathVFXScale.floatValue, 0, 10);
        EditorGUILayout.HelpBox("VFX Scaling works on some effects better than others due to how some authors make VFX.", MessageType.Warning);
    }
#endif
}
