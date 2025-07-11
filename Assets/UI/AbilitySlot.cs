using UnityEngine;
using UnityEngine.UIElements;

public class AbilitySlot : PurchaseSlot
{
    private Ability abilityData;

    public AbilitySlot(VisualElement _purchaseSlot, Ability _abilityData) : base(_purchaseSlot)
    {
        if (_abilityData == null)
        {
            Debug.LogError($"{nameof(_abilityData)} is null in {GetType().Name}!");
            return;
        }
        this.abilityData = _abilityData;
    }

    public override void SubscribeHoverPriceLabel(Label label)
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
            label.text = abilityData.PurchasePrice.ToString();
        };

        purchaseButton.RegisterCallback<MouseEnterEvent>(onHoverEnter);
    }

    public override void UnsubscribeHoverPriceLabel(Label label)
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
