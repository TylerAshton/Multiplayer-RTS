using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New RadialHeal Ability", menuName = "Abilities/RadialHeal")]
public class RadialHeal : Ability<ICharacterAbilityUser>
{
    [SerializeField] float radius = 1f;
    [SerializeField] float healAmount = 1f;
    [SerializeField] private LayerMask layerMask = 1 << 7;

    protected override void ActivateTyped(ICharacterAbilityUser _user)
    {
        //_user.NAnimator.SetTrigger($"{AnimationTrigger}"); // TODO: Add an effect shiz
        OnUseTyped(_user);
    }

    protected override void DebugDrawingTyped(ICharacterAbilityUser _user)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUseTyped(ICharacterAbilityUser _user)
    {
        Transform castPositionTransform = GetCastPositionTransform(_user);
        HealArea(castPositionTransform, _user);
        

    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        EditorGUILayout.HelpBox("VFX and animations don't work here yet!", MessageType.Warning);

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
