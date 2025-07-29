using FMODUnity;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Ability<T> : Ability where T : IAbilityUser 
{
    

    protected abstract void OnCastTyped(T _user);
    /// <summary>
    /// Default behaviour for OnCast such as playing sounds or running animations.
    /// </summary>
    /// <param name="_user"></param>
    private void OnCastDefault(IAbilityUser _user)
    {
        if (CastSound != null && !CastSound.SoundEvent.IsNull)
        {
            SoundSpawner.Instance.PlaySoundEffectRpc(CastSound.ID, _user.Transform.position);
        }
    }
    public override void OnCast(IAbilityUser _user)
    {
        if (_user is T tUser)
        {
            OnCastDefault(_user);
            OnCastTyped(tUser);
        }
        else
        {
            Debug.LogError($"Attempted to activate an ability with {_user.GetType()} " +
                $"instead of {typeof(T)}.");
        }
    }

    protected abstract void OnApexTyped(T _user);
    private void OnApexDefault(IAbilityUser _user)
    {
        if (!ApexSound.SoundEvent.IsNull)
        {
            SoundSpawner.Instance.PlaySoundEffectRpc(ApexSound.ID, _user.Transform.position);
        }
    }
    public override void OnApex(IAbilityUser _user)
    {
        if (_user is T tUser)
        {
            OnApexDefault(_user);
            OnApexTyped(tUser);
        }
        else
        {
            Debug.LogError($"Attempted to OnUse an ability with {_user.GetType()} " +
                $"instead of {typeof(T)}.");
        }
    }

    /// <summary>
    /// Called everyframe to demonstrate debugging stuff such a gizmo range diagrams. Will not run on build
    /// </summary>
    /// <param name="_user"></param>
    /// <param name="_abilityPositions"></param>
    protected abstract void DebugDrawingTyped(T _user);
    public override void DebugDrawing(IAbilityUser _user)
    {
        if (_user is T tUser)
        {
            DebugDrawingTyped(tUser);
        }
        else
        {
            Debug.LogError($"Attempted to DebugDrawing an ability with {_user.GetType()} " +
                $"instead of {typeof(T)}.");
        }
    }

    protected Transform GetCastPositionTransform(T _user)
    {
        return _user.AbilityPositions[CastPositionName];
    }

/*    protected virtual void CopySubclassTo(Ability<T> _target)
    {
        CopyBaseTo(_target);
    }*/
}