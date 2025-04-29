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

    public abstract void DebugDrawing(GameObject _user, List<Transform> _abilityPositions);
}