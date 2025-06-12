using System;
using Unity.Services.Matchmaker.Models;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using ObjectField = UnityEditor.UIElements.ObjectField;

[CustomPropertyDrawer(typeof(Ability), true)]
public class PropertyDrawerTest : PropertyDrawer
{
    private VisualElement ContentPanel;
    private Label CaretLabel;
    private bool IsExpanded = true;
    private override VisualElement CreatePropertyGUI(SerializedProperty Property)
    {
        VisualElement inspector = new();
        inspector.AddToClassList("panel");

        return BuildUI(inspector, Property);
    }

    private VisualElement BuildUI(VisualElement RootElement, SerializedProperty Property)
    {
        VisualElement titleContainer = new();
        titleContainer.AddToClassList("align-horizontal");
        CaretLabel = new(">");
        CaretLabel.style.fontSize = 18;
        CaretLabel.AddToClassList(IsExpanded ? "rotate-90" : "rotate-0");
        titleContainer.Add(CaretLabel);
        Label title = new("Shooting COnfiguration");
        title.AddToClassList("header");

        titleContainer.Add(title);
        titleContainer.RegisterCallback<ClickEvent>(HandleTitleClick);

        RootElement.Add(titleContainer);

        // Show different panel based on if we have a serializedObject selected
        if (Property.objectReferenceValue == null)
        {
            ContentPanel = BuildNoShootPanel(RootElement, Property);
            RootElement.Add(ContentPanel);
        }
        else
        {
            ContentPanel = BuildShootConfigBox(RootElement, Property);
            RootElement.Add(ContentPanel);
        }

        ContentPanel.AddToClassList(IsExpanded ? "expanded" : "collapsed");

        return RootElement;
    }

    /// <summary>
    /// What we're showing if we have a serizedObject selected
    /// </summary>
    /// <param name="rootElement"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private VisualElement BuildShootConfigBox(VisualElement rootElement, SerializedProperty property)
    {
        VisualElement shootConfigBox = new();
        shootConfigBox.name = "shoot-config-box";

        shootConfigBox.Add(BuildObjectField(rootElement, property));
        Button deleteButton = new Button(() => DeleteSO(property));
        deleteButton.text = "Delete";
        deleteButton.AddToClassList("danger");
        deleteButton.AddToClassList("align-right");
        deleteButton.AddToClassList("mb-8");
        shootConfigBox.Add(deleteButton);

        SerializedObject shootConfigSO = new(property.objectReferenceValue);

        Label bulletBehaviour = new("Gun/Bullet Interaction");
        bulletBehaviour.AddToClassList("bold");
        shootConfigBox.Add(bulletBehaviour);


        // Get and display the fields

/*        EnumField shootTypeField = new("Shoot Type", ShootType.FromGun);
        shootTypeField.BindProperty(shootConfigSO.FindProperty("ShootType"));
        shootConfigBox.Add(shootTypeField);*/

        // Only show these two fields if the isHitScan toggle is set to true

        ObjectField bulletPrefab = new("Bullet Prefab");
        bulletPrefab.BindProperty(shootConfigSO.FindProperty("BulletPrefab"));

        FloatField bulletSpawnForceField = new("Bullet Force");
        bulletSpawnForceField.BindProperty(shootConfigSO.FindProperty("BulletSpawnForce"));
  

        SerializedProperty isHitscan = shootConfigSO.FindProperty("IsHitscan");
        Toggle isHitscanToggle = new("Is Hitscan Gun");
        isHitscanToggle.RegisterValueChangedCallback((changeEvent) =>
        {
            if (changeEvent.newValue)
            {
                bulletPrefab.AddToClassList("hidden");
                bulletSpawnForceField.AddToClassList("hidden");
            }
            else
            {
                bulletPrefab.RemoveFromClassList("hidden");
                bulletSpawnForceField.RemoveFromClassList("hidden");
            }
        });
        isHitscanToggle.BindProperty(isHitscan);
        shootConfigBox.Add(isHitscanToggle); // TODO: 12:48

    }

    private async object DeleteSO(SerializedProperty property)
    {
        string path = AssetDatabase.GetAssetPath(property.objectReferenceInstanceIDValue);
        property.objectReferenceValue = null;
        property.serializedObject.ApplyModifiedProperties();

        // If we don't defer this then we do a null vs null check in OnEditorGUI and need to rebuild the whole
        await Task.Delay(100);
        AssetDatabase.DeleteAsset(path);
    }

    /// <summary>
    /// What we're showing if we don't have a serlizedObject selected
    /// </summary>
    /// <param name="rootElement"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private VisualElement BuildNoShootPanel(VisualElement rootElement, SerializedProperty property)
    {
        VisualElement noShootConfigBox = new();
        noShootConfigBox.name = "no-shoot-config-box";

        Label noShootConfigLabel = new Label("No Shoot Config Exists!");
        noShootConfigLabel.AddToClassList("mb-8");
        noShootConfigBox.Add(noShootConfigLabel);

        noShootConfigBox.Add(new Label("Create a new one with name"));
        VisualElement horizontalBox = new();
        horizontalBox.AddToClassList("align-horizontal");

        TextField soNameField = new();
        soNameField.AddToClassList("flex-grow");

        Button createButton = new(() => CreateShootConfig(soNameField.text, property));
        createButton.text = "Create";

        horizontalBox.Add(soNameField);
        horizontalBox.Add(createButton);

        noShootConfigBox.Add(horizontalBox);

        Label selectExistingLabel = new Label("Select Existing");
        selectExistingLabel.AddToClassList("pt-4");
        selectExistingLabel.AddToClassList("mt-4");
        selectExistingLabel.AddToClassList("thin-border-top");
        noShootConfigBox.Add(selectExistingLabel);

        noShootConfigBox.Add(BuildObjectField(rootElement, property));

        return noShootConfigBox;
    }

    /// <summary>
    /// Displays the fields for a sieralizedObject such as a prexisting ability
    /// </summary>
    /// <param name="rootElement"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private VisualElement BuildObjectField(VisualElement rootElement, SerializedProperty property)
    {
        UnityEditor.UIElements.ObjectField shootConfigObjectField = new("Shoot Config");
        shootConfigObjectField.objectType = typeof(Ability);
        shootConfigObjectField.BindProperty(property.serializedObject.FindProperty("ShootConfig"));

        Ability currentValue = property.objectReferenceValue as Ability;
        shootConfigObjectField.RegisterValueChangedCallback((changeEvent) =>
        {
            //ChangeEvents are dispatched AFTER the change. so you have to use the if() check
            if (changeEvent.newValue != currentValue)
            {
                HandleChangeShootConfig(changeEvent, rootElement, property);
            }
        });

        return shootConfigObjectField;
    }

    /// <summary>
    /// Rebuild UI with change to the serializedObject
    /// </summary>
    /// <param name="changeEvent"></param>
    /// <param name="rootElement"></param>
    /// <param name="property"></param>
    private void HandleChangeShootConfig(ChangeEvent<UnityEngine.Object> changeEvent, VisualElement rootElement, SerializedProperty property)
    {
        rootElement.Clear();
        BuildUI(rootElement, property.serializedObject.FindProperty("ShootConfig"));
    }

    private void HandleTitleClick(ClickEvent evt)
    {
        if (IsExpanded)
        {
            CaretLabel.RemoveFromClassList("rotate-90");
            CaretLabel.AddToClassList("Rotate-0");

            ContentPanel.RemoveFromClassList("expanded");
            ContentPanel.AddToClassList("collasped");
        }

        else
        {
            CaretLabel.RemoveFromClassList("Rotate-0");
            CaretLabel.AddToClassList("rotate-90");

            ContentPanel.AddToClassList("expanded");
            ContentPanel.RemoveFromClassList("collasped");
        }
        
        IsExpanded = !IsExpanded;
    }

    /// <summary>
    /// Creates a new scriptable object
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="property"></param>
    private void CreateShootConfig(string Name, SerializedProperty property)
    {
        // Create new ability
        Ability newAbility = ScriptableObject.CreateInstance<Ability>();
        // Save new ability file
        AssetDatabase.CreateAsset(newAbility, Name + ".asset");

        // Apply changes to new ability file
        property.objectReferenceValue = newAbility;
        property.serializedObject.ApplyModifiedProperties();
        property.serializedObject.Update();
    }
}
