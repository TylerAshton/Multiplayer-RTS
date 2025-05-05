using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Effect/Frenzy")]
public class FrenzyEffect : Effect
{
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float healthRegen = 1f;
    
    public override void OnEnd(EffectManager _effectManager)
    {
        _effectManager.AbilityManager.SetAttackSpeed(1);
    }

    public override void OnStart(EffectManager _effectManager)
    {
        _effectManager.AbilityManager.SetAttackSpeed(attackSpeed);
    }

    public override void OnUpdate(EffectManager _effectManager)
    {
        _effectManager.gameObject.GetComponent<Health>().Heal(healthRegen);
    }
}
