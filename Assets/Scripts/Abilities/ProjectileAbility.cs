using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "Abilities/Projectile")]
public class ProjectileAbility : Ability<IAbilityUser>
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private ProjectileStats projectileStats;
    protected override void ActivateTyped(IAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{AnimationTrigger}");
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {
        
    }

    protected override void OnUseTyped(IAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        GameObject spawnedProjectile = Instantiate(GetProjectileBlueprint(), castPositionTransform.position, Quaternion.identity); // TODO: Change the index of ability positions and in fact how we store said positions. Dict?
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        BulletProjectile bulletProjectile = spawnedProjectile.GetComponent<BulletProjectile>();
        bulletProjectile.ApplyProjectileStats(projectileStats);
        bulletProjectile.LaunchProjectile(_user.Transform.forward);
        spawnedProjectile.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }

    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldProjectileStats = _so.FindProperty("projectileStats");
        EditorGUILayout.PropertyField(fieldProjectileStats);

        /*SerializedProperty fieldProjectilePrefab = _so.FindProperty("projectile");
        fieldProjectilePrefab.objectReferenceValue = EditorGUILayout.ObjectField("Projectile Prefab", fieldProjectilePrefab.objectReferenceValue, typeof(GameObject), false);
*/    }

    private GameObject GetProjectileBlueprint()
    {
        GameObject projectile = Resources.Load<GameObject>("Blueprints/BPBullet");
        return projectile;
    }
}
