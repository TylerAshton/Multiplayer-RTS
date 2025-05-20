using System.Collections.Generic;
using UnityEngine;

public class Building : Unit, IAbilityUser
{
    public Animator Animator => throw new System.NotImplementedException();

    public Transform Transform => transform;

    private AbilityPositionManager abilityPositionManager;

    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    public EffectManager EffectManager => throw new System.NotImplementedException();
}
