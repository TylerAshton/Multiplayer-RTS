using UnityEngine;

public interface ICharacterAbilityUser : IUnitAbilityUser
{
    NetCodeAnimationManager NAnimator { get; }
    EffectManager EffectManager { get; }

    void Lunge(float distance, Vector3 direction, float lungeDuration);
}
