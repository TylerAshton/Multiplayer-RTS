/*using UnityEngine;
using UnityEngine.UIElements;

public class HealSlot : PurchaseSlot
{
    int healCost;
    int healAmount;
    protected override int Price => healCost;
    public HealSlot(VisualElement _purchaseSlot, IShopUser _shopUser, int _healCost, int _healAmount) : base(_purchaseSlot, _shopUser)
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

    protected override void SubmitPurchaseRequest()
    {
        throw new System.NotImplementedException();
    }
}*/
