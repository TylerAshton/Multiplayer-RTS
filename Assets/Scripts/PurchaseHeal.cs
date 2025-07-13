using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Purchase", menuName = "ShopMisc")]
public class PurchaseHeal : Purchasable
{
    [SerializeField] private int healAmount;
    public override void ExecutePurchase(IShopUser _shopUser)
    {
        PointManager.Instance.RemovePoints(_shopUser.PlayerID, this.price);
        _shopUser.ChampionHealth.Heal(healAmount);
    }
}
