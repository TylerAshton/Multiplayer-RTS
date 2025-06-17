using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Ability", menuName = "Abilities/Summon")]
public class SummonAbility : Ability<IAbilityUser>
{
    [SerializeField] private GameObject spawnee;
    [SerializeField] private GameObject spawnVFX;
    [SerializeField] private float maxDispersion = 5f;
    [SerializeField] private float minDisperstion = 5f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    protected override void ActivateTyped(IAbilityUser _user)
    {
        Vector3 castPosition = _user.Transform.position + offset;

        // Generate random XZ offset within the specified dispersion range
        float offsetX = Random.Range(-maxDispersion, maxDispersion);
        float offsetZ = Random.Range(-maxDispersion, maxDispersion);

        // Ensure offset is not too close
        Vector2 offsetXZ = new Vector2(offsetX, offsetZ);
        if (offsetXZ.magnitude < minDisperstion)
        {
            offsetXZ = offsetXZ.normalized * minDisperstion;
        }

        Vector3 spawnPosition = new Vector3(
            castPosition.x + offsetXZ.x,
            castPosition.y,
            castPosition.z + offsetXZ.y
        );
        GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {

    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        throw new System.NotImplementedException();
    }

    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldSpawnee = _so.FindProperty("spawnee");
        fieldSpawnee.objectReferenceValue = EditorGUILayout.ObjectField("Spawnee Prefab", fieldSpawnee.objectReferenceValue, typeof(GameObject), false);

        SerializedProperty fieldSpawnVFX = _so.FindProperty("spawnVFX");
        fieldSpawnVFX.objectReferenceValue = EditorGUILayout.ObjectField("Spawn VFX Prefab", fieldSpawnVFX.objectReferenceValue, typeof(GameObject), false);

        SerializedProperty fieldMaxDispersion = _so.FindProperty("maxDispersion");
        fieldMaxDispersion.floatValue = EditorGUILayout.FloatField("Max Dispersion", fieldMaxDispersion.floatValue);

        SerializedProperty fieldMinDisperstion = _so.FindProperty("minDisperstion");
        fieldMinDisperstion.floatValue = EditorGUILayout.FloatField("Min Dispersion", fieldMinDisperstion.floatValue);

        SerializedProperty fieldOffset = _so.FindProperty("offset");
        EditorGUILayout.PropertyField(fieldOffset, new GUIContent("Offset"));
    }
}
