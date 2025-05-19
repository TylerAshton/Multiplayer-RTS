using UnityEngine;

/// <summary>
/// This is a generic class which lets C# be happy with storing and polymorphing 
/// all kinds of abilities
/// </summary>
public abstract class Ability : ScriptableObject
{
    [SerializeField] private float castTime = 1f;
    [SerializeField] private AbilityPosition castPositionName = AbilityPosition.Centre;
    [SerializeField] private string animationTrigger;
    [SerializeField] private Sprite icon;

    public float CastTime => castTime;
    public AbilityPosition CastPositionName => castPositionName;
    public string AnimationTrigger => animationTrigger;
    public Sprite Icon => icon;

    public abstract void Activate(IAbilityUser _user);
    public abstract void OnUse(IAbilityUser _user);
    public abstract void DebugDrawing(IAbilityUser _user);
}
