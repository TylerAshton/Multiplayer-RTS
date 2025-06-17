using UnityEditor;
using UnityEngine;

/// <summary>
/// This is a generic class which lets C# be happy with storing and polymorphing 
/// all kinds of abilities
/// </summary>
[System.Serializable]
public abstract class Ability : ScriptableObject
{
    [SerializeField] private string abilityID = string.Empty;
    [SerializeField] private string abilityName = string.Empty;
    public string AbilityID => abilityID;
    public string AbilityName => abilityName;
    [SerializeField] private float castTime = 1f;
    [SerializeField] private AbilityPosition castPositionName = AbilityPosition.Centre;
    [SerializeField] private string animationTrigger;
    [SerializeField] private Sprite icon;
    [SerializeField] private int abilityCost = 0;
    [SerializeField] private float cooldown = 0f;

    public float Cooldown => cooldown;

    public int AbilityCost => abilityCost;
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

    /// <summary>
    /// Used by the AbilityEditorWindow to draw the inspector for abilities.
    /// </summary>
    /// <param name="so"></param>
    public virtual void DrawInspector(SerializedObject so)
    {
        SerializedProperty fieldID = so.FindProperty("abilityID");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (fieldID.stringValue == "")
        {
            EditorGUILayout.HelpBox("Ability ID Can't be null", MessageType.Error);
        }

        SerializedProperty fieldName = so.FindProperty("abilityName");
        fieldName.stringValue = EditorGUILayout.TextField("Name", fieldName.stringValue);

        SerializedProperty fieldAnimationTrigger = so.FindProperty("animationTrigger");
        fieldAnimationTrigger.stringValue = EditorGUILayout.TextField("Animation Trigger", fieldAnimationTrigger.stringValue); // TODO: Remove?
                                                                                                                               //EditorGUILayout.HelpBox("Honestly don't animationTrigger touch this without a dev.", MessageType.Warning);

        SerializedProperty fieldCastTime = so.FindProperty("castTime");
        fieldCastTime.floatValue = EditorGUILayout.Slider("Cast Time", fieldCastTime.floatValue, 0, 10);

        SerializedProperty fieldCooldown = so.FindProperty("cooldown");
        fieldCooldown.floatValue = EditorGUILayout.Slider("Cooldown", fieldCooldown.floatValue, 0, 60);

        SerializedProperty fieldAbilityCost = so.FindProperty("abilityCost");
        fieldAbilityCost.intValue = EditorGUILayout.IntField("Ability Cost", fieldAbilityCost.intValue);

        if (fieldAbilityCost.intValue < 0)
        {
            EditorGUILayout.HelpBox("Ability Cost cannot be negative.", MessageType.Error);
        }



        SerializedProperty fieldCastPos = so.FindProperty("castPositionName");
        fieldCastPos.enumValueIndex = EditorGUILayout.Popup("Cast Position", fieldCastPos.enumValueIndex, fieldCastPos.enumDisplayNames);

        SerializedProperty fieldIcon = so.FindProperty("icon");
        fieldIcon.objectReferenceValue = EditorGUILayout.ObjectField("Ability Icon", fieldIcon.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
    }
/*
    public abstract Ability Clone();

    /// <summary>
    /// Protected copier to copy the ability's properties to another ability instance.
    /// </summary>
    /// <param name="_target"></param>
    protected void CopyBaseTo(Ability _target)
    {
        _target.abilityID = this.abilityID;
        _target.castTime = this.castTime;
        _target.castPositionName = this.castPositionName;
        _target.animationTrigger = this.animationTrigger;
        _target.icon = this.icon;
        _target.abilityCost = this.abilityCost;
        _target.cooldown = this.cooldown;
    }

    protected abstract void CopySubclassTo(Ability _target);*/
}
