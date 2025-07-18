using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New RadialHeal Ability", menuName = "Abilities/RadialHeal")]
public class RadialHeal : Ability<ICharacterAbilityUser>, VfxObject
{
    [SerializeField] float radius = 1f;
    [SerializeField] float healAmount = 1f;
    [SerializeField] private LayerMask layerMask = 1 << 7;
    [SerializeField] private GameObject healVFX;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxScale = 1f;
    [SerializeField] private float vfxDuration = 5f;
    [SerializeField] private float slowAmount = 7;

    private const int minVFXRadius = 0;
    private const int maxVFXRadius = 10;
    protected override string animationTrigger => "RadialAbility";

    public GameObject VfxPrefab => healVFX;

    public Vector3 VfxOffset => vfxOffset;

    public float VfxScale => vfxScale;

    public float VfxDuration => vfxDuration;

    protected override void ActivateTyped(ICharacterAbilityUser _user)
    {
        _user.NAnimator.SetTrigger($"{animationTrigger}");

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

    protected override void OnUseTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        HealArea(castPositionTransform, _user);
        //SpawnVFX(castPositionTransform.position + vfxOffset);
        VFXSpawner.Instance.AbilityVfxRpc(id, castPositionTransform.position);



    }

    private GameObject GetVfxBlueprint()
    {
        GameObject vfxPrefab = Resources.Load<GameObject>("Blueprints/BPVFX");
        return vfxPrefab;
    }

/*    private void SpawnVFX(Vector3 _spawnPos)
    {
        GameObject vfxObj = Instantiate(GetVfxBlueprint(), _spawnPos, Quaternion.identity);
        vfxObj.GetComponent<NetworkObject>().Spawn();
        vfxObj.GetComponent<VFXSpawner>().SpawnVFXRpc();

        if (healVFX == null)
        {
            Debug.LogError($"{nameof(healVFX)} is null in {this.name}");
            return;
        }

        GameObject spawnedVfx = Instantiate(healVFX, _spawnPos, Quaternion.identity);
        spawnedVfx.transform.position += vfxOffset;
        VFXScaler.ScaleParticles(vfxScale, spawnedVfx);
    }*/

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

        SerializedProperty fieldHealVFX = _so.FindProperty("healVFX");
        fieldHealVFX.objectReferenceValue = EditorGUILayout.ObjectField("Heal VFX", fieldHealVFX.objectReferenceValue, typeof(GameObject), false);
        if (fieldHealVFX.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Heal VFX must be assigned!", MessageType.Error);
        }

        SerializedProperty fieldVFXOffset = _so.FindProperty("vfxOffset");
        EditorGUILayout.PropertyField(fieldVFXOffset, new GUIContent("Vfx Offset"));

        SerializedProperty fieldHealVFXScale = _so.FindProperty("vfxScale");
        fieldHealVFXScale.floatValue = EditorGUILayout.Slider("Bullet VFX Scale", fieldHealVFXScale.floatValue, minVFXRadius, maxVFXRadius);
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
