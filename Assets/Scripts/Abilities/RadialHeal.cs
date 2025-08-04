using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New RadialHeal Ability", menuName = "Abilities/RadialHeal")]
public class RadialHeal : Ability<ICharacterAbilityUser>
{
    [SerializeField] float radius = 1f;
    [SerializeField] float healAmount = 1f;
    [SerializeField] private LayerMask layerMask = 1 << 7;
    [SerializeField] private VfxObject healVFX;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxScale = 1f;
    [SerializeField] private float vfxDuration = 5f;
    [SerializeField] private float slowAmount = 7;

    private const int minVFXRadius = 0;
    private const int maxVFXRadius = 10;
    protected override string animationTrigger => "RadialAbility";

    public VfxObject HealVfx => healVFX;

    public Vector3 VfxOffset => vfxOffset;

    public float VfxScale => vfxScale;

    public float VfxDuration => vfxDuration;

    protected override void OnCastTyped(ICharacterAbilityUser _user)
    {
        _user.AnimTriggerManager.TrySetTrigger($"{animationTrigger}");

        // Apply slow
        StatModifyer statModifyer = new StatModifyer(StatType.MoveSpeed, -slowAmount);
        List<StatModifyer> statModifyers = new List<StatModifyer>();
        statModifyers.Add(statModifyer);

        Effect newEffect = new Effect(CastTime, statModifyers);

        _user.EffectManager.AddEffect(newEffect);
    }

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnApexTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        HealArea(castPositionTransform, _user);
        VFXSpawner.Instance.SpawnVfxObjectRpc(healVFX.ID, castPositionTransform.position);




    }

    private GameObject GetVfxBlueprint()
    {
        GameObject vfxPrefab = Resources.Load<GameObject>("Blueprints/BPVFX");
        return vfxPrefab;
    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldSlowAmount = _so.FindProperty("slowAmount");
        EditorGUILayout.PropertyField(fieldSlowAmount);
        if (fieldSlowAmount.floatValue < 0)
        {
            EditorGUILayout.HelpBox("Slow amount must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldRadius = _so.FindProperty("radius");
        fieldRadius.floatValue = EditorGUILayout.FloatField("Radius", fieldRadius.floatValue);
        if (fieldRadius.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Radius must be greater than 0!", MessageType.Error);
        }

        SerializedProperty fieldHealing = _so.FindProperty("healAmount");
        fieldHealing.floatValue = EditorGUILayout.FloatField("Heal Amount", fieldHealing.floatValue);
        if (fieldHealing.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Heal amount must be greater than 0!", MessageType.Error);
        }

        BeaconUtility.DrawStat<VfxObject>(_so, "healVFX", false);
    }
#endif

    private void HealArea(Transform _centre, ICharacterAbilityUser _user)
    {
        Collider[] hits = Physics.OverlapSphere(_centre.position, radius, layerMask);

        foreach(Collider hit in hits)
        {
            if(!hit.TryGetComponent<IFaction>(out IFaction faction))
            {
                continue;
                
            }

            if (faction.Faction != _user.IFaction.Faction)
            {
                continue;
            }

            // Healing juice
            if (hit.TryGetComponent(out Health health))
            {
                health.Heal(healAmount);
            }
        }
    }
}
