using UnityEngine;

public interface ICharacterAbilityUser : IAbilityUser
{
    NetCodeAnimationManager NAnimator { get; }
    EffectManager EffectManager { get; }
}
