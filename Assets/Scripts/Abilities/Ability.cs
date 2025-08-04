using FMODUnity;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This is a generic class which lets C# be happy with storing and polymorphing 
/// all kinds of abilities
/// </summary>
[System.Serializable]
public abstract class Ability : Purchasable, Inspectorable
{
    public string AbilityName => this.name; // This probably isn't needed but cba to refactor some stuff
    [SerializeField] private float castTime = 1f;
    [SerializeField] private AbilityPosition castPositionName = AbilityPosition.Centre;
    [SerializeField] private int abilityCost = 0;
    [SerializeField] private float cooldown = 0f;
    [SerializeField] private Ability successor; [Tooltip("The ability that this ability will be replaced with if its added.")]
    [SerializeField] private SoundObject castSound;
    [SerializeField] private SoundObject apexSound;
    protected virtual string animationTrigger => null;

    public float Cooldown => cooldown;

    public int AbilityCost => abilityCost;
    public float CastTime => castTime;
    public AbilityPosition CastPositionName => castPositionName;
    public Ability Successor => successor;
    public SoundObject CastSound => castSound;
    public SoundObject ApexSound => apexSound;

    //public string PurchaseID => abilityID;

    /*    private void OnValidate()
        {
            purchaseID = abilityID; // NOTE: this is temp until we merge the IDs together
        }*/

    /// <summary>
    /// Returns true if the user can use this ability. I.e, has enough points
    /// NOTE: Doesn't take cooldown into account as cooldowns are handled by the user
    /// </summary>
    /// <param name="_user"></param>
    /// <returns></returns>
    public bool CanUse(IAbilityUser _user)
    {
        // Cost checker // TODO: Enable ability cost checking
        int currentPoints = PointManager.Instance.GetPoints(_user.OwnerID);

        if (currentPoints < this.AbilityCost)
        {
            return false;
        }

        return true;
    }

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

    public override bool ExecutePurchase(IShopUser _shopUser)
    {
        if (!CanPurchase(_shopUser))
        {
            Debug.LogError("Cannot purchase conditions aren't met!");
            return false;
        }


        PointManager.Instance.RemovePoints(_shopUser.PlayerID, this.price);
        _shopUser.ChampionAbilityManager.AddAbility(this, 0);
        return true;
    }

    /// <summary>
    /// Phantom function form for Activate which allows different types of ability classes to type cast
    /// themselves when they are called without necessarily calling them. For example when accessed
    /// via a mixed list
    /// </summary>
    /// <param name="_user"></param>
    public abstract void OnCast(IAbilityUser _user);

    /// <summary>
    /// Phantom function form for OnUse which allows different types of ability classes to type cast
    /// themselves when they are called without necessarily calling them. For example when accessed
    /// via a mixed list
    /// </summary>
    /// <param name="_user"></param>
    public abstract void OnApex(IAbilityUser _user);

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

        SerializedProperty fieldSuccessor = _so.FindProperty("successor");
        EditorGUILayout.PropertyField(fieldSuccessor, new GUIContent("Successor Ability"));

        BeaconUtility.DrawStat<SoundObject>(_so, "castSound", true);

        BeaconUtility.DrawStat<SoundObject>(_so, "apexSound", true);


    }
#endif

#if UNITY_EDITOR
    protected void DrawStat<T>(SerializedObject _so, string _fieldName) where T : UnityEngine.Object, Inspectorable
    {
        SerializedProperty fieldBaseAbilityStat = _so.FindProperty(_fieldName);

        if (fieldBaseAbilityStat == null)
        {
            Debug.LogError($"SerializedProperty is null in {GetType().Name}. Please assign a valid SerializedProperty.");
            return;
        }

        EditorGUILayout.PropertyField(fieldBaseAbilityStat);

        if (fieldBaseAbilityStat.objectReferenceValue != null)
        {
            DrawStatValues<T>(fieldBaseAbilityStat);
        }
        else
        {
            EditorGUILayout.HelpBox($"Stats field cannot be null!", MessageType.Error);
        }

    }

    protected void DrawStatValues<T>(SerializedProperty _sp) where T : UnityEngine.Object, Inspectorable
    {
        if (_sp.objectReferenceValue == null)
        {
            Debug.LogError($"SerializedProperty is null in {GetType().Name}. Please assign a valid SerializedProperty.");
        }

        SerializedObject statsSO = new SerializedObject(_sp.objectReferenceValue);
        T stat = _sp.objectReferenceValue as T;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{nameof(T)}", EditorStyles.boldLabel);

        statsSO.Update();

        stat.DrawInspector(statsSO);

        statsSO.ApplyModifiedProperties();   
    }

#endif
}
