using UnityEngine;
using UnityEngine.UIElements;

public abstract class PurchaseSlot
{
    protected VisualElement purchaseSlot;
    protected Button purchaseButton;
    protected Label purchaseLabel;

    protected EventCallback<MouseEnterEvent> onHoverEnter;
    protected EventCallback<ClickEvent> onClickEvent;


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

    public abstract void SubscribeHoverPriceLabel(Label label);

    public abstract void UnsubscribeHoverPriceLabel(Label label);
}
