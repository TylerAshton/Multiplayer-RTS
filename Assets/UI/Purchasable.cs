using UnityEngine;
using UnityEngine.Serialization;

public abstract class Purchasable : RegistryItem
{
    [SerializeField] protected int price;
    public int Price => price;

    [FormerlySerializedAs("purchaseAbleIcon")]
    [SerializeField] protected Sprite icon;

    public Sprite Icon => icon;

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
