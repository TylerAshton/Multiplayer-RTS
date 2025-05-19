using System.Collections.Generic;
using UnityEngine;

public abstract class Ability<T> : Ability where T : IAbilityUser 
{
    

    protected abstract void ActivateTyped(T _user);
    public override void Activate(IAbilityUser _user)
    {
        if (_user is T tUser)
        {
            ActivateTyped(tUser);
        }
        else
        {
            Debug.LogError($"Attempted to activate an ability with {_user.GetType()} " +
                $"instead of {typeof(T)}.");
        }
    }

    protected abstract void OnUseTyped(T _user);
    public override void OnUse(IAbilityUser _user)
    {
        if (_user is T tUser)
        {
            OnUseTyped(tUser);
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
}