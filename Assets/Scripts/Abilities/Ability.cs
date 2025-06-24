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
    /// <param name="_so"></param>
    /// 
#if UNITY_EDITOR // Will crash if this is not wrapped in UNITY_EDITOR
    public virtual void DrawInspector(SerializedObject _so)
    {
        SerializedProperty fieldID = _so.FindProperty("abilityID");
        fieldID.stringValue = EditorGUILayout.TextField("ID", fieldID.stringValue);
        if (fieldID.stringValue == "")
        {
            EditorGUILayout.HelpBox("Ability ID Can't be null", MessageType.Error);
        }

        SerializedProperty fieldName = _so.FindProperty("abilityName");
        fieldName.stringValue = EditorGUILayout.TextField("Name", fieldName.stringValue);

        SerializedProperty fieldAnimationTrigger = _so.FindProperty("animationTrigger");
        fieldAnimationTrigger.stringValue = EditorGUILayout.TextField("Animation Trigger", fieldAnimationTrigger.stringValue); // TODO: Remove?
                                                                                                                               //EditorGUILayout.HelpBox("Honestly don't animationTrigger touch this without a dev.", MessageType.Warning);

        SerializedProperty fieldCastTime = _so.FindProperty("castTime");
        fieldCastTime.floatValue = EditorGUILayout.Slider("Cast Time", fieldCastTime.floatValue, 0, 10);

        SerializedProperty fieldCooldown = _so.FindProperty("cooldown");
        fieldCooldown.floatValue = EditorGUILayout.Slider("Cooldown", fieldCooldown.floatValue, 0, 60);

        SerializedProperty fieldAbilityCost = _so.FindProperty("abilityCost");
        fieldAbilityCost.intValue = EditorGUILayout.IntField("Ability Cost", fieldAbilityCost.intValue);

        if (fieldAbilityCost.intValue < 0)
        {
            EditorGUILayout.HelpBox("Ability Cost cannot be negative.", MessageType.Error);
        }



        SerializedProperty fieldCastPos = _so.FindProperty("castPositionName");
        fieldCastPos.enumValueIndex = EditorGUILayout.Popup("Cast Position", fieldCastPos.enumValueIndex, fieldCastPos.enumDisplayNames);

        SerializedProperty fieldIcon = _so.FindProperty("icon");
        fieldIcon.objectReferenceValue = EditorGUILayout.ObjectField("Ability Icon", fieldIcon.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
    }
#endif
}
