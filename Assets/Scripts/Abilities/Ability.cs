using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [SerializeField] private string abilityName;
    [SerializeField] private float castTime = 1f;
    [SerializeField] protected string animationTrigger;
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;
    public float CastTime => castTime;

    public abstract void Activate(GameObject user, Animator _animator);

    public abstract void OnUse(GameObject _user, List<Transform> _abilityPositions);

    /// <summary>
    /// Called everyframe to demonstrate debugging stuff such a gizmo range diagrams. Will not run on build
    /// </summary>
    /// <param name="_user"></param>
    /// <param name="_abilityPositions"></param>
    public abstract void DebugDrawing(GameObject _user, List<Transform> _abilityPositions);
}