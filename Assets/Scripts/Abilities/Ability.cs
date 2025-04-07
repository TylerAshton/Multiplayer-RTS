using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    [SerializeField] private float castTime = 1f;
    public float CastTime => castTime;

    public abstract void Activate(GameObject user, Animator _animator);
}