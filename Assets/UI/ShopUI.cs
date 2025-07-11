using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopUI : MonoBehaviour
{
    private const int purchaseCap = 5;
    private const int abilityCap = 4;

    [SerializeField] private int healthCost = 500;
    [SerializeField] private int healthAmount = 1000;

    private VisualElement heal;
    private VisualElement ability1;
    private VisualElement ability2;
    private VisualElement ability3;
    private VisualElement ability4;


    private Label label;

    private List<VisualElement> purchaseUIElements = new List<VisualElement>();
    [SerializeField] private Ability[] purchasableAbilities = new Ability[abilityCap];
    private List<PurchaseSlot> purchaseSlots = new List<PurchaseSlot>();


    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        label = root.Q<Label>("PriceLabel");
        heal = root.Q<VisualElement>("Heal");
        ability1 = root.Q<VisualElement>("Ability1");
        ability2 = root.Q<VisualElement>("Ability2");
        ability3 = root.Q<VisualElement>("Ability3");
        ability4 = root.Q<VisualElement>("Ability4");

        purchaseUIElements.Add(heal);
        purchaseUIElements.Add(ability1);
        purchaseUIElements.Add(ability2);
        purchaseUIElements.Add(ability3);
        purchaseUIElements.Add(ability4);

        if (purchasableAbilities.Length > abilityCap)
        {
            Debug.LogError($"{nameof(purchasableAbilities)} {purchasableAbilities.Length} exceeds ability cap of {abilityCap}!");
            return;
        }

        CreatePurchaseSlots();




    }

    /// <summary>
    /// Populates the purchaseSlots list with PurchaseSlot objects based on the UI elements defined in the purchaseUIElements list.
    /// </summary>
    private void CreatePurchaseSlots()
    {
        int abilityIndex = 0;

        foreach (VisualElement element in purchaseUIElements)
        {
            PurchaseSlot purchaseSlot = null;

            if (element.name.StartsWith("Ability")) // Maybe better to add this to the element class tag instead?
            {
                if (abilityIndex >= abilityCap)
                {
                    Debug.LogError($"Ability cap has been exceeded by ui element {element.name}!");
                    continue;
                }

                if (abilityIndex >= purchasableAbilities.Length)
                {
                    Debug.LogError($"Not enough purchasable abilities for {element.name}! Expected {abilityCap}, found {purchasableAbilities.Length}!");
                    continue;
                }

                if (purchasableAbilities[abilityIndex] == null)
                {
                    Debug.LogError($"Purchasable ability at index {abilityIndex} is null for {element.name}!");
                    continue;
                }

                purchaseSlot = new AbilitySlot(element, purchasableAbilities[abilityIndex]);
                abilityIndex++;
            }

            else if (element.name == "Heal")
            {
                purchaseSlot = new HealSlot(element, healthCost, healthAmount);
            }


            if (purchaseSlot == null)
            {
                Debug.LogError($"Could not find slot type for {element.name}");
                continue;
            }
            purchaseSlots.Add(purchaseSlot);
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
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.UnsubscribeActions();
        }
    }

    private void ButtonActionsSubscribe()
    {
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.SubscribeActions();
        }

    }

    private void CostDisplay(int _cost)
    {
        label.text = $"Cost: {_cost}";
    }
}
