using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Construct Ability", menuName = "Abilities/Construct")]
public class Construct : Ability<IConstructionPad>, IVfxObject
{
    [SerializeField] private GameObject spawnee;
    [SerializeField] private GameObject spawnVFX;

    public GameObject VfxPrefab => spawnVFX;

    public Vector3 VfxOffset => Vector3.zero; // NOTE: In the future perhaps we'd wanna add offsets en such to 
                                               // Consturctions but wasn't needed.. yet.

    public float VfxScale => 1f;

    public float VfxDuration => 5f;

    protected override void OnCastTyped(IConstructionPad _user)
    {
        Vector3 spawnPosition = _user.Transform.position;


        /*GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        vfx.GetComponent<NetworkObject>().Spawn();*/
        VFXSpawner.Instance.SpawnAbilityVfxRpc(this.id, spawnPosition);
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();

        _user.ConstructionPad.SetOccupiedBuilding(summoned);
        summoned.GetComponent<Health>().OnDeath += _user.ConstructionPad.ShowBuildPad;

        // Select new unit
        RTSPlayer.Instance.UnitManager.SelectUnit(summoned.GetComponent<SelectableObject>());
    }

    protected override void DebugDrawingTyped(IConstructionPad _user)
    {
        
    }

    protected override void OnApexTyped(IConstructionPad _user)
    {
        
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
    }
#endif
}
