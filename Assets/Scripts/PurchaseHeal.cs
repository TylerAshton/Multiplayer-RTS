using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Purchase", menuName = "ShopMisc")]
public class PurchaseHeal : Purchasable
{
    [SerializeField] private int healAmount;
    public override void ExecutePurchase(IShopUser _shopUser)
    {
        _shopUser.ChampionHealth.Heal(healAmount);
    }
}
