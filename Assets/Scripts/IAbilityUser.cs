using System.Collections.Generic;
using UnityEngine;

public interface IAbilityUser
{
    Animator Animator { get; }
    Transform Transform { get; }
    IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions { get; }
    EffectManager EffectManager { get; }
}
