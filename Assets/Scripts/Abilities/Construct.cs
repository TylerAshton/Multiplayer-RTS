using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Construct Ability", menuName = "Abilities/Construct")]
public class Construct : Ability<IConstructionPad>
{
    [SerializeField] private GameObject spawnee;

    [SerializeField] private VfxObject vfxObject;
    public VfxObject VfxPrefab => vfxObject;

    protected override void OnCastTyped(IConstructionPad _user)
    {
        Vector3 spawnPosition = _user.Transform.position;


        /*GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        vfx.GetComponent<NetworkObject>().Spawn();*/
        //VFXSpawner.Instance.SpawnAbilityVfxRpc(this.id, spawnPosition);
        GameObject summoned = Instantiate(spawnee, spawnPosition, Quaternion.identity);
        summoned.GetComponent<NetworkObject>().Spawn();

        _user.ConstructionPad.SetOccupiedBuilding(summoned);
        summoned.GetComponent<Health>().OnDeath += _user.ConstructionPad.ShowBuildPad;

        // Select new unit
        RTSPlayer.Instance.UnitManager.SelectUnit(summoned.GetComponent<SelectableObject>());

        VFXSpawner.Instance.SpawnVfxObjectRpc(vfxObject.ID, spawnPosition, _user.OwnerID);
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

        BeaconUtility.DrawStat<VfxObject>(_so, "vfxObject", false);
    }
#endif
}
