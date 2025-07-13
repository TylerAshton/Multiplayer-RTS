using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PurchaseSlot
{
    protected VisualElement purchaseSlot;
    protected Button purchaseButton;
    protected Label purchaseLabel;
    protected IShopUser shopUser;
    protected Purchasable purchasable;

    protected EventCallback<MouseEnterEvent> onHoverEnter;
    protected EventCallback<ClickEvent> onClickEvent;

    public event Action<PurchaseSlot> OnAttemptedPurchase;


    public PurchaseSlot(VisualElement _purchaseSlot, IShopUser _shopUser, Purchasable _purchasable)
    {
        if (_purchaseSlot == null)
        {
            Debug.LogError($"{nameof(_purchaseSlot)} is null in {GetType().Name}!");
            return;
        }

        this.purchaseSlot = _purchaseSlot;

        this.purchaseButton = _purchaseSlot.Q<Button>("AbilityButton");

        if (this.purchaseButton == null)
        {
            Debug.LogError($"{nameof(purchaseButton)} was not found in {nameof(_purchaseSlot)} for {GetType().Name}!");
            return;
        }

        this.purchaseLabel = _purchaseSlot.Q<Label>("AbilityLabel");

        if (this.purchaseLabel == null)
        {
            Debug.LogError($"{nameof(purchaseLabel)} was not found in {nameof(_purchaseSlot)} for {GetType().Name}!");
            return;
        }

        if (_shopUser == null)
        {
            Debug.LogError($"{nameof(_shopUser)} is null in {GetType().Name}!");
            return;
        }

        this.shopUser = _shopUser;

        if (_purchasable == null)
        {
            Debug.LogError($"{nameof(_purchasable)} is null in {GetType().Name}!");
            return;
        }
        this.purchasable = _purchasable;
    }

    protected void SubmitPurchaseRequest()
    {
        shopUser.ShopPurchaseManager.HandlePurchaseRequestRpc(purchasable.PurchaseID);
    }

    public void SubscribePurchaseButtonClickEvent()
    {
        if (onClickEvent != null) // This shouldn't be stopped as it's possible to sub more than once legally
        {
            purchaseButton.UnregisterCallback(onClickEvent);
        }
        onClickEvent = evt =>
        {
            if (purchasable.CanPurchase(shopUser)) { SubmitPurchaseRequest(); }
        };
        purchaseButton.RegisterCallback(onClickEvent);
    }

    public void UnsubscribePurchaseButtonClickEvent()
    {
        if (onClickEvent == null)
        {
            Debug.LogError($"{nameof(onClickEvent)} is not assigned in {GetType().Name}!");
            return;
        }
        purchaseButton.UnregisterCallback(onClickEvent);
        onClickEvent = null;


    }


    public void SubscribeHoverPriceLabel(Label label)
    {
        if (label == null)
        {
            Debug.LogError($"{nameof(label)} is null in {GetType().Name}!");
            return;
        }

        if (onHoverEnter != null) // This shouldn't be stopped as it's possibile to sub more than once legally
        {
            purchaseButton.UnregisterCallback(onHoverEnter);
        }

        onHoverEnter = evt =>
        {
            label.text = purchasable.Price.ToString();
        };

        purchaseButton.RegisterCallback<MouseEnterEvent>(onHoverEnter);
    }

    public void UnsubscribeHoverPriceLabel(Label label)
    {
        if (label == null)
        {
            Debug.LogError($"{nameof(label)} is null in {GetType().Name}!");
            return;
        }

        if (onHoverEnter == null)
        {
            Debug.LogError($"{nameof(onHoverEnter)} is not assigned in {GetType().Name}!");
            return;
        }

        purchaseButton.UnregisterCallback<MouseEnterEvent>(onHoverEnter);
        onHoverEnter = null;
    }


}
