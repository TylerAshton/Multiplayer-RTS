using UnityEngine;
using UnityEngine.UIElements;

public abstract class PurchaseSlot
{
    protected VisualElement purchaseSlot;
    protected Button purchaseButton;
    protected Label purchaseLabel;

    protected EventCallback<MouseEnterEvent> onHoverEnter;
    protected EventCallback<ClickEvent> onClickEvent;

    protected abstract int Price { get; }


    public PurchaseSlot(VisualElement _purchaseSlot)
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
            label.text = Price.ToString();
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
        }

        purchaseButton.UnregisterCallback<MouseEnterEvent>(onHoverEnter);
        onHoverEnter = null;
    }
}
