using UnityEngine;
using UnityEngine.UIElements;

public class HealSlot : PurchaseSlot
{
    int healCost;
    int healAmount;
    public HealSlot(VisualElement _purchaseSlot, int _healCost, int _healAmount) : base(_purchaseSlot)
    {
        if (_healCost < 0)
        {
            Debug.LogError($"{nameof(_healCost)} can't be a negative in {GetType().Name}!");
            return;
        }

        if (_healAmount <= 0)
        {
            Debug.LogError($"{nameof(_healAmount)} must be greater than 0 in {GetType().Name}!");
            return;
        }

        this.healCost = _healCost;
        this.healAmount = _healAmount;
    }

    public override void SubscribeActions()
    {
        throw new System.NotImplementedException();
    }

    public override void UnsubscribeActions()
    {
        throw new System.NotImplementedException();
    }
}
