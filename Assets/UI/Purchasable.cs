using UnityEngine;

public abstract class Purchasable : ScriptableObject
{
    [SerializeField] protected string purchaseID;
    public string PurchaseID => purchaseID;

    private int price;
    public int Price => price;

    public virtual bool CanPurchase(IShopUser _shopUser)
    {
        if (_shopUser == null)
        {
            Debug.LogError($"{nameof(_shopUser)} is null!");
            return false;
        }

        if (_shopUser.Points < Price)
        {
            Debug.LogWarning($"Not enough gold to purchase! Required: {Price}, Available: {_shopUser.Points}");
            return false;
        }

        return true;
    }
    public abstract void ExecutePurchase(IShopUser _shopUser);
}
