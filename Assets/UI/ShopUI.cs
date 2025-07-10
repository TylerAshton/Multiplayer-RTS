using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopUI : MonoBehaviour
{
    private const int abilityCap = 4;

    private VisualElement heal;
    private VisualElement ability1;
    private VisualElement ability2;
    private VisualElement ability3;
    private VisualElement ability4;


    private Label label;

    private List<VisualElement> abilitySlots = new List<VisualElement>();
    [SerializeField] private Ability[] purchasableAbilities = new Ability[abilityCap];


    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        label = root.Q<Label>("PriceLabel");
        heal = root.Q<VisualElement>("Heal");
        ability1 = root.Q<VisualElement>("Ability1");
        ability2 = root.Q<VisualElement>("Ability2");
        ability3 = root.Q<VisualElement>("Ability3");
        ability4 = root.Q<VisualElement>("Ability4");

        abilitySlots.Add(ability1);
        abilitySlots.Add(ability2);
        abilitySlots.Add(ability3);
        abilitySlots.Add(ability4);

        if (purchasableAbilities.Length > abilityCap)
        {
            Debug.LogError($"{nameof(purchasableAbilities)} {purchasableAbilities.Length} exceeds ability cap of {abilityCap}!");
            return;
        }

        for (int i = 0; i < purchasableAbilities.Length && purchasableAbilities[i] != null; i++)
        {
            //Debug.Log(i);
        }
    }

    private void OnEnable()
    {
        ButtonActionsSubscribe();
    }

    private void OnDisable()
    {
        ButtonActionsUnsubscribe();
    }

    private void ButtonActionsUnsubscribe()
    {
        //healthButton.clicked -= HealthButton_clicked;
    }

    private void ButtonActionsSubscribe()
    {
        healthButton.RegisterCallback<PointerEnterEvent>(evt => CostDisplay(500));

        for (int i = 0; i < purchasableAbilities.Length; i++)
        {
            int index = i;
            abilitySlots[index].RegisterCallback<PointerEnterEvent>(evt => CostDisplay(purchasableAbilities[index].PurchasePrice));
        }
        /*
                healthButton.clicked += HealthButton_clicked;
                healthButton.RegisterCallback<PointerEnterEvent>(evt => OnHealthHover());*/

    }

    private void CostDisplay(int _cost)
    {
        label.text = $"Cost: {_cost}";
    }
}
