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
#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldSpawnee = _so.FindProperty("spawnee");
        fieldSpawnee.objectReferenceValue = EditorGUILayout.ObjectField("Spawnee Prefab", fieldSpawnee.objectReferenceValue, typeof(GameObject), false);
        if (fieldSpawnee.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Spawnee Prefab must be assigned.", MessageType.Error);
        }

        SerializedProperty fieldSpawnVFX = _so.FindProperty("spawnVFX");
        fieldSpawnVFX.objectReferenceValue = EditorGUILayout.ObjectField("Spawn VFX Prefab", fieldSpawnVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldSpawnVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Spawn VFX Prefab must be assigned.", MessageType.Error);
        }

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
