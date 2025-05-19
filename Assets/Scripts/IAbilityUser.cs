using System.Collections.Generic;
using UnityEngine;

public interface IAbilityUser
{
    Animator Animator { get; }
    Transform Transform { get; }
    Dictionary<AbilityPosition, Transform> AbilityPositions { get; }
    EffectManager EffectManager { get; }
}
