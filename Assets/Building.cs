using System.Collections.Generic;
using UnityEngine;

public class Building : Unit, IAbilityUser
{
    public Animator Animator => throw new System.NotImplementedException();

    public Transform Transform => throw new System.NotImplementedException();

    public Dictionary<AbilityPosition, Transform> AbilityPositions => throw new System.NotImplementedException();

    public EffectManager EffectManager => throw new System.NotImplementedException();
}
