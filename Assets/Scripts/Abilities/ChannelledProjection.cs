using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Channelled Projection Ability", menuName = "Abilities/Channelled Projection")]
public class ChannelledProjection : Ability<ICharacterAbilityUser>
{
    [SerializeField] private float slowAmount;
    [SerializeField] private ProjectionStats channelStats;
    [SerializeField] private bool isAttached = false;
    protected override string animationTrigger => "ProjectionAbility";
    protected override void OnCastTyped(ICharacterAbilityUser _user)
    {

    }

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {
        
    }

    protected override void OnApexTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        //Quaternion rotation = isAttached ? Quaternion.identity : castPositionTransform.rotation; // If we're not attached. Use the cast position rotation
        Quaternion rotation = castPositionTransform.rotation;

        GameObject newProjection = Instantiate(GetProjectionBlueprint(), castPositionTransform.position, rotation);
        newProjection.GetComponent<NetworkObject>().Spawn();

        if (isAttached)
        {
            newProjection.GetComponent<NetworkParent>().SetParent(castPositionTransform);
        }

        //Applying stats to the projection
        ProjectionManager projectionManager = newProjection.GetComponent<ProjectionManager>();
        projectionManager.ApplyProjectionStatsWithID(channelStats.ID);
        HitboxManager hitboxManager = newProjection.GetComponent<HitboxManager>();
        hitboxManager.ApplyHitboxStatsWithID(channelStats.HitboxStats.ID);
        newProjection.GetComponent<IFaction>().Faction = _user.IFaction.Faction;
    }

    private GameObject GetProjectionBlueprint()
    {
        GameObject projectile = Resources.Load<GameObject>("Blueprints/BPProjection");
        return projectile;
    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldIsAttached = _so.FindProperty("isAttached");
        fieldIsAttached.boolValue = EditorGUILayout.Toggle("Attach?", fieldIsAttached.boolValue);

        DrawStat<BaseAbilityStat>(_so, "channelStats");
    }
#endif
}
