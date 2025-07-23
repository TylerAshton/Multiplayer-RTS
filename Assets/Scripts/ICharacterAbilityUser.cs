using UnityEngine;

public interface ICharacterAbilityUser : IUnitAbilityUser
{
    AnimationTriggerManager AnimTriggerManager { get; }
    EffectManager EffectManager { get; }

    void Lunge(float distance, Vector3 direction, float lungeDuration);
}
