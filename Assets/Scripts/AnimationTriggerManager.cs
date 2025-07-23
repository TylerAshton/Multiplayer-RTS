using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Master list of all the standard animation triggers used by our game
/// </summary>
public readonly struct AnimTriggers
{
    public const string OnDeath = "Death";
    public const string OnHit = "OnHit";
    public const string OnRevive = "Revive";
    public const string OnProjectile = "ProjectileAbility";
    public const string OnProjection = "ProjectionAbility";
    public const string OnBuff = "BuffAbility";
    public const string OnMelee = "MeleeAbility";
    public const string OnRadial = "RadialAbility";

}

/// <summary>
/// Used by other scripts to perform standard animations that are commonly shared across multiple in game characters
/// </summary>
public class AnimationTriggerManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private List<string> animationTriggers = new List<string>();

    private Health health;

    private void Awake()
    {


        GetTriggers();
        SetupTriggers();
    }

    private void SetupTriggers()
    {
        foreach (string _trigger in animationTriggers)
        {

        }
    }

    private void SubscribeHealth()
    {

    }

    private void GetTriggers()
    {
        animationTriggers.Clear();

        AnimatorControllerParameter[] parameters = animator.parameters;

        animationTriggers = parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger).Select(p => p.name).ToList();
    }
}
