using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UpgradeAbility : Ability<IAbilityUser>
{
    [SerializeField] private List<Ability> upgradePackage = new List<Ability>();
    [SerializeField] private int tabIndex = 0;
    protected override void ActivateTyped(IAbilityUser _user)
    {
        foreach (Ability ability in upgradePackage)
        {
            _user.AbilityManager.AddAbility(ability, tabIndex);
        }

        _user.AbilityManager.RemoveAbility(this);
    }

    protected override void DebugDrawingTyped(IAbilityUser _user)
    {

    }

    protected override void OnUseTyped(IAbilityUser _user)
    {

    }

#if UNITY_EDITOR
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldEffect = _so.FindProperty("upgradePackage");
        EditorGUILayout.PropertyField(fieldEffect, true);
        if (fieldEffect.arraySize == 0)
        {
            EditorGUILayout.HelpBox("Upgrade Package must contain at least one ability.", MessageType.Error);
        }

        SerializedProperty fieldTabIndex = _so.FindProperty("tabIndex");
        fieldTabIndex.intValue = EditorGUILayout.IntField("Tab Index", fieldTabIndex.intValue);
        if (fieldTabIndex.intValue < 0)
        {
            EditorGUILayout.HelpBox("Tab Index cannot be negative.", MessageType.Error);
        }
        if (fieldTabIndex.intValue > 3)
        {
            EditorGUILayout.HelpBox("Tab Index cannot be greater than 3.", MessageType.Error);
        }
    }
#endif
}
