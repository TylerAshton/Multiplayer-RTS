using System.Collections.Generic;
using UnityEngine;

public interface IAbilityUser
{
    Transform Transform { get; }
    IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions { get; }   
    Transform CastTarget { get; }
    IFaction IFaction { get; }

    public void SetTarget(Transform castTarget);
    public void ClearTarget();
}
