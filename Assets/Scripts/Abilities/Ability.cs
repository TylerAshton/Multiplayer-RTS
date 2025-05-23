using UnityEngine;

/// <summary>
/// This is a generic class which lets C# be happy with storing and polymorphing 
/// all kinds of abilities
/// </summary>
public abstract class Ability : ScriptableObject
{
    [SerializeField] private string abilityID = string.Empty;
    public string AbilityID => abilityID;
    [SerializeField] private float castTime = 1f;
    [SerializeField] private AbilityPosition castPositionName = AbilityPosition.Centre;
    [SerializeField] private string animationTrigger;
    [SerializeField] private Sprite icon;

    public float CastTime => castTime;
    public AbilityPosition CastPositionName => castPositionName;
    public string AnimationTrigger => animationTrigger;
    public Sprite Icon => icon;

    /// <summary>
    /// Phantom function form for Activate which allows different types of ability classes to type cast
    /// themselves when they are called without necessarily calling them. For example when accessed
    /// via a mixed list
    /// </summary>
    /// <param name="_user"></param>
    public abstract void Activate(IAbilityUser _user);

    /// <summary>
    /// Phantom function form for OnUse which allows different types of ability classes to type cast
    /// themselves when they are called without necessarily calling them. For example when accessed
    /// via a mixed list
    /// </summary>
    /// <param name="_user"></param>
    public abstract void OnUse(IAbilityUser _user);

    /// <summary>
    /// Phantom function form for DebugDrawing which allows different types of ability classes to type cast
    /// themselves when they are called without necessarily calling them. For example when accessed
    /// via a mixed list
    /// </summary>
    /// <param name="_user"></param>
    public abstract void DebugDrawing(IAbilityUser _user);
}
