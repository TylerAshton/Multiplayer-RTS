using UnityEditor;
using UnityEngine;

/// <summary>
/// This is a generic class which lets C# be happy with storing and polymorphing 
/// all kinds of abilities
/// </summary>
[System.Serializable]
public abstract class Ability : Purchasable
{
    public string AbilityName => this.name; // This probably isn't needed but cba to refactor some stuff
    [SerializeField] private float castTime = 1f;
    [SerializeField] private AbilityPosition castPositionName = AbilityPosition.Centre;
    [SerializeField] private string animationTrigger;
    [SerializeField] private int abilityCost = 0;
    [SerializeField] private float cooldown = 0f;

    public float Cooldown => cooldown;

    public int AbilityCost => abilityCost;
    public float CastTime => castTime;
    public AbilityPosition CastPositionName => castPositionName;
    public string AnimationTrigger => animationTrigger;

    //public string PurchaseID => abilityID;

/*    private void OnValidate()
    {
        purchaseID = abilityID; // NOTE: this is temp until we merge the IDs together
    }*/

    public override bool CanPurchase(IShopUser _shopUser)
    {
        if (base.CanPurchase(_shopUser) == false)
        {
            return false;
        }

        if (_shopUser.ChampionAbilityManager.CheckAbility(this))
        {
            Debug.LogWarning($"Ability {this.AbilityName} is already owned by the user.");
            return false;
        }

        return true;
    }

    public override void ExecutePurchase(IShopUser _shopUser)
    {
        if (!CanPurchase(_shopUser))
        {
            Debug.LogError("Cannot purchase conditions aren't met!");
            return;
        }


        PointManager.Instance.RemovePoints(_shopUser.PlayerID, this.price);
        _shopUser.ChampionAbilityManager.AddAbility(this, 0);
    }

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
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

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

        
    }
#endif

#if UNITY_EDITOR
    protected void DrawStat(SerializedProperty _sp)
    {
        if (_sp.objectReferenceValue == null)
        {
            Debug.LogError($"SerializedProperty is null in {GetType().Name}. Please assign a valid SerializedProperty.");
        }

        SerializedObject statsSO = new SerializedObject(_sp.objectReferenceValue);
        BaseAbilityStat stat = (BaseAbilityStat)_sp.objectReferenceValue;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{stat.name}", EditorStyles.boldLabel);

        statsSO.Update();

        stat.DrawInspector(statsSO);

        statsSO.ApplyModifiedProperties();
    }

#endif
}
