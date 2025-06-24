using System.Collections.Generic;
using UnityEngine;

public class Building : Unit, IAbilityUser
{
    public NetCodeAnimationManager NAnimator => throw new System.NotImplementedException();

    public Transform Transform => transform;

    public EffectManager EffectManager => throw new System.NotImplementedException();

    public IFaction IFaction => this;
}
