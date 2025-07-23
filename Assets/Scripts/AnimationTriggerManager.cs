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
/// Used by other scripts to perform standard animations that are commonly shared across multiple in game characters.
/// However, not all characters will have these animations nor the components used. So it'll bind to whatever is ready.
/// It should be noted that all these animations being on every character on in scope hence this sytem.
/// </summary>
public class AnimationTriggerManager : MonoBehaviour
{
    private Animator animator;
    private NetCodeAnimationManager nAnimator;
    private Health health;

    private List<string> animationTriggers = new List<string>();

    private void Awake()
    {
        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(Health)} in gameobject: {gameObject.name}!");
        }
        if (!TryGetComponent<NetCodeAnimationManager>(out nAnimator))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(NetCodeAnimationManager)} in gameobject: {gameObject.name}!");
        }
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(Animator)} in gameobject: {gameObject.name}!");
        }
    }

    private void OnEnable()
    {
        GetTriggers();
        SubscribeEventTriggers();
    }
    private void OnDisable()
    {
        UnsubscribeEventTriggers();
    }

    /// <summary>
    /// Sets up the triggers for events such as health taking damage. Will only bind what's available in the animationTriggers
    /// </summary>
    private void SubscribeEventTriggers()
    {
        foreach (string _trigger in animationTriggers)
        {
            switch (_trigger)
            {
                case AnimTriggers.OnHit:
                    health.OnHit += Health_OnHit;
                    break;
                case AnimTriggers.OnDeath:
                    health.OnDeath += Health_OnDeath;
                    break;
                case AnimTriggers.OnRevive:
                    health.OnRevive += Health_OnRevive;
                    break;
            }
        }
    }

    private void UnsubscribeEventTriggers()
    {
        foreach (string _trigger in animationTriggers)
        {
            switch (_trigger)
            {
                case AnimTriggers.OnHit:
                    health.OnHit -= Health_OnHit;
                    break;
                case AnimTriggers.OnDeath:
                    health.OnDeath -= Health_OnDeath;
                    break;
                case AnimTriggers.OnRevive:
                    health.OnRevive -= Health_OnRevive;
                    break;
            }
        }
    }

    private void Health_OnRevive()
    {
        RunTrigger(AnimTriggers.OnRevive);
    }

    private void Health_OnHit()
    {
        RunTrigger(AnimTriggers.OnHit);
    }

    private void Health_OnDeath()
    {
        RunTrigger(AnimTriggers.OnDeath);
    }

    private void GetTriggers()
    {
        animationTriggers.Clear();

        AnimatorControllerParameter[] parameters = animator.parameters;

        animationTriggers = parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger).Select(p => p.name).ToList();
    }

    public void RunTrigger(string _triggerName)
    {
        if (!animationTriggers.Contains(_triggerName))
        {
            Debug.LogWarning($"{_triggerName} was not found in {animationTriggers} within gameobject, {gameObject.name}, and so will not be ran");
            return;
        }

        nAnimator.SetTrigger(_triggerName);
    }
}
