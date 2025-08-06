using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;



public class ShopUI : MonoBehaviour
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

    private ShopPurchaseManager shopPurchaseManager;




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
            Debug.LogError($"{GetType().Name} requires {nameof(IShopUser)} within gameobject: {championGO.name}!");
            return;
        }
        if (!championGO.TryGetComponent<ShopPurchaseManager>(out shopPurchaseManager))
        {
            Debug.LogError($"{nameof(ShopPurchaseManager)} is required for {GetType().Name} in gameobject {championGO.name}!");
            return;
        }
        InitUIVariables();
        DrawPurchaseSlots();
    }

    private void Update()
    {
        foreach (PurchaseSlot slot in purchaseSlots)
        {
            slot.OnUpdate();
        }
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

    /// <summary>
    /// Populates the purchaseSlots list with PurchaseSlot objects based on the UI elements defined in the purchaseUIElements list.
    /// </summary>
    private void DrawPurchaseSlots()
    {
        ButtonActionsUnsubscribe();
        purchaseSlots.Clear();

        foreach (VisualElement _purchaseUIElement in purchaseUIElements)
        {
            _purchaseUIElement.style.visibility = Visibility.Hidden;
        }

        if (purchaseUIElements.Count < purchasables.Count)
        {
            Debug.LogError($"Too many {nameof(Purchasable)}s to fit in our {purchaseUIElements.Count} purchase slots!");
            return;
        }

        for (int i = 0; i < purchaseUIElements.Count; i++)
        {
            VisualElement purchaseUIElement = purchaseUIElements[i];
            Purchasable purchasable = (i < purchasables.Count) ? purchasables[i] : null; // gotta do this shit or it'll error on empty

            if (purchasable is Ability _abiltiy)
            {
                purchasable = TryGetSuccessorAbility(_abiltiy);
            }

            if (purchaseUIElement == null)
            {
                Debug.LogError($"Index {i} is null in {nameof(purchaseUIElements)}!");
            }

            // Hide button if we're out of purchasables
            if (purchasable == null)
            {
                //purchaseUIElement.style.visibility = Visibility.Hidden;
                Debug.LogWarning($"Index {i} is null in {nameof(purchasables)}!");
                continue;
            }

            purchaseUIElement.style.visibility = Visibility.Visible;
            PurchaseSlot newPurchaseSlot = new PurchaseSlot(purchaseUIElement, championShopUser, purchasable);

            purchaseSlots.Add(newPurchaseSlot);
        }

        ButtonActionsSubscribe();
    }

    private bool IsAbilityAlreadyPurchased(Ability _abiltiy)
    {
        bool isOwned = championShopUser.ChampionAbilityManager.CheckAbility(_abiltiy);

        if (isOwned)
        {
            return true;
        }

        if (_abiltiy.Successor == null)
        {
            return false;
        }

        return IsAbilityAlreadyPurchased(_abiltiy.Successor);

        




    }

    /// <summary>
    /// If the ability is already purchased returns the successor of the parsed ability.
    /// However, if the output is already purchased it will return of the successor of such. However if there is none then returns null.
    /// </summary>
    /// <param name="_ability"></param>
    /// <returns></returns>
    private Purchasable TryGetSuccessorAbility(Ability _ability)
    {
        if (!IsAbilityAlreadyPurchased(_ability))
        {
            return _ability;
        }

        // Is purchased get successor


        Ability successorAbility;
        successorAbility = _ability.Successor;

        if (successorAbility == null)
        {
            return null;
        }

        return TryGetSuccessorAbility(successorAbility);
    }

    private void OnEnable()
    {
        //ButtonActionsSubscribe();
        shopPurchaseManager.OnSuccessfullPurchase += DrawPurchaseSlots;
    }

    private void OnDisable()
    {
        //ButtonActionsUnsubscribe();
        shopPurchaseManager.OnSuccessfullPurchase -= DrawPurchaseSlots;
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
