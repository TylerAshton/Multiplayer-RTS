using UnityEngine;

public interface ICharacterAbilityUser : IAbilityUser
{
    NetCodeAnimationManager NAnimator { get; }
    EffectManager EffectManager { get; }

    void Lunge(float distance, Vector3 direction);
}
