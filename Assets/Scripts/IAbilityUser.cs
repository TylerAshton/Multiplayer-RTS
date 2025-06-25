using System.Collections.Generic;
using UnityEngine;

public interface IAbilityUser
{
    NetCodeAnimationManager NAnimator { get; }
    Transform Transform { get; }
    IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions { get; }
    EffectManager EffectManager { get; }
    Transform CastTarget { get; }
    public void SetTarget(Transform castTarget);
    public void ClearTarget();

    IFaction IFaction { get; }
}
