using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;



public class ShopUI : NetworkBehaviour
{
    private const int purchaseCap = 5;
    private const int abilityCap = 4;

    private GameObject championGO;
    private IShopUser championShopUser;
    private ChampionAbilityManager championAbilityManager;
    private Health championHealth;
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
        championGO = transform.parent.gameObject;

        if (championGO == null)
        {
            Debug.LogError($"{GetType().Name} requires a parent Champion for gameobject: {gameObject.name}");
            return;
        }

        if (!championGO.TryGetComponent<IShopUser>(out championShopUser))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(IShopUser)} within gameobject: {gameObject.name}!");
            return;
        }
        if (!championGO.TryGetComponent<ChampionAbilityManager>(out championAbilityManager))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(ChampionAbilityManager)} within gameobject: {gameObject.name}!");
            return;
        }




        InitUIVariables();
        CreatePurchaseSlots();
    }

    private void InitUIVariables()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        label = root.Q<Label>("PriceLabel");
        if (label == null)
        {
            Debug.LogError($"{nameof(label)} was not found in the root visual element!");
            return;
        }
        heal = root.Q<VisualElement>("Heal");
        if (heal == null)
        {
            Debug.LogError($"{nameof(heal)} was not found in the root visual element!");
            return;
        }
        ability1 = root.Q<VisualElement>("Ability1");
        if (ability1 == null)
        {
            Debug.LogError($"{nameof(ability1)} was not found in the root visual element!");
            return;
        }
        ability2 = root.Q<VisualElement>("Ability2");
        if (ability2 == null)
        {
            Debug.LogError($"{nameof(ability2)} was not found in the root visual element!");
            return;
        }
        ability3 = root.Q<VisualElement>("Ability3");
        if (ability3 == null)
        {
            Debug.LogError($"{nameof(ability3)} was not found in the root visual element!");
            return;
        }
        ability4 = root.Q<VisualElement>("Ability4");
        if (ability4 == null)
        {
            Debug.LogError($"{nameof(ability4)} was not found in the root visual element!");
            return;
        }

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
    }

    private void RelaySlotPurchaseRequest(PurchaseSlot _purchaseSlot)
    {
        if (_purchaseSlot is AbilitySlot abilitySlot) // NOTE: This is fine as we'll only ever have 2 purchase elements within our scope of game
        {
            championShopUser.ShopPurchaseManager.HandleAbilityPurchaseRequestRpc(abilitySlot.AbilityData.AbilityID);
        }
        else if (_purchaseSlot is HealSlot)
        {
            championShopUser.ShopPurchaseManager.HandleHealPurchaseRequestRpc(healthCost, healthAmount);
        }
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

                purchaseSlot = new AbilitySlot(element, championShopUser, purchasableAbilities[abilityIndex]);
                abilityIndex++;
            }

            else if (element.name == "Heal")
            {
                purchaseSlot = new HealSlot(element, championShopUser, healthCost, healthAmount);
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

    private void ButtonActionsSubscribe()
    {
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.SubscribeHoverPriceLabel(label);
            slot.SubscribePurchaseButtonClickEvent();
            slot.OnAttemptedPurchase += RelaySlotPurchaseRequest;
        }
    }
    private void ButtonActionsUnsubscribe()
    {
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.UnsubscribeHoverPriceLabel(label);
            slot.UnsubscribePurchaseButtonClickEvent();
            slot.OnAttemptedPurchase -= RelaySlotPurchaseRequest;
        }
    }

    private void CostDisplay(int _cost)
    {
        label.text = $"Cost: {_cost}";
    }
}
