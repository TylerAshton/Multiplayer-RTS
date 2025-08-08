using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Purchase", menuName = "ShopMisc")]
public class PurchaseHeal : Purchasable
{
    [SerializeField] private int healAmount;
    public override bool ExecutePurchase(IShopUser _shopUser)
    {
        if (PointManager.Instance.GetPoints(_shopUser.PlayerID) < this.price)
        {
            return false;
        }

        PointManager.Instance.RemovePoints(_shopUser.PlayerID, this.price);
        _shopUser.ChampionHealth.Heal(healAmount);
        return true;
    }
}
