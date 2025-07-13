using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;



public class ShopUI : NetworkBehaviour
{

    private GameObject championGO;
    private IShopUser championShopUser;

    private VisualElement heal;
    private VisualElement ability1;
    private VisualElement ability2;
    private VisualElement ability3;
    private VisualElement ability4;

    private Label priceLabel;

    private List<VisualElement> purchaseUIElements = new List<VisualElement>();
    [SerializeField] private List<Purchasable> purchasables = new List<Purchasable>();
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

        InitUIVariables();
        CreatePurchaseSlots();
    }

    private void InitUIVariables()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        priceLabel = root.Q<Label>("PriceLabel");
        if (priceLabel == null)
        {
            Debug.LogError($"{nameof(priceLabel)} was not found in the root visual element!");
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

        if (purchasables.Count > purchaseUIElements.Count)
        {
            Debug.LogError($"{nameof(purchasables)} {purchasables.Count} is bigger than the amount of buttons we have {purchaseUIElements.Count}!");
            return;
        }
    }

/*    private void RelaySlotPurchaseRequest(PurchaseSlot _purchaseSlot)
    {
        if (_purchaseSlot is AbilitySlot abilitySlot) // NOTE: This is fine as we'll only ever have 2 purchase elements within our scope of game
        {
            championShopUser.ShopPurchaseManager.HandleAbilityPurchaseRequestRpc(abilitySlot.AbilityData.AbilityID);
        }
        else if (_purchaseSlot is HealSlot)
        {
            championShopUser.ShopPurchaseManager.HandleHealPurchaseRequestRpc(healthCost, healthAmount);
        }
    }*/

    

    /// <summary>
    /// Populates the purchaseSlots list with PurchaseSlot objects based on the UI elements defined in the purchaseUIElements list.
    /// </summary>
    private void CreatePurchaseSlots()
    {
        if (purchaseUIElements.Count < purchasables.Count)
        {
            Debug.LogError($"Too many {nameof(Purchasable)}s to fit in our {purchaseUIElements.Count} purchase slots!");
            return;
        }

        for (int i = 0; i < purchaseUIElements.Count; i++)
        {
            Purchasable purchasable = purchasables[i];
            VisualElement purchaseUIElement = purchaseUIElements[i];

            if (purchasable == null)
            {
                Debug.LogError($"Index {i} is null in {nameof(purchasables)}!");
            }
            if (purchaseUIElement == null)
            {
                Debug.LogError($"Index {i} is null in {nameof(purchaseUIElements)}!");
            }

            PurchaseSlot newPurchaseSlot = new PurchaseSlot(purchaseUIElement, championShopUser, purchasables[i]);

            purchaseSlots.Add(newPurchaseSlot);
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
            slot.SubscribeHoverPriceLabel(priceLabel);
            slot.SubscribePurchaseButtonClickEvent();
/*            slot.OnAttemptedPurchase += RelaySlotPurchaseRequest;*/
        }
    }
    private void ButtonActionsUnsubscribe()
    {
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.UnsubscribeHoverPriceLabel(priceLabel);
            slot.UnsubscribePurchaseButtonClickEvent();
/*            slot.OnAttemptedPurchase -= RelaySlotPurchaseRequest;*/
        }
    }

    private void CostDisplay(int _cost)
    {
        priceLabel.text = $"Cost: {_cost}";
    }
}
