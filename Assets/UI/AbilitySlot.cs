/*using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class AbilitySlot : PurchaseSlot
{
    private Ability abilityData;
    public Ability AbilityData => abilityData;  
    protected override int Price => abilityData.PurchasePrice;

    public AbilitySlot(VisualElement _purchaseSlot, IShopUser _shopUser, Ability _abilityData) : base(_purchaseSlot, _shopUser)
    {
        if (_abilityData == null)
        {
            Debug.LogError($"{nameof(_abilityData)} is null in {GetType().Name}!");
            return;
        }
        this.abilityData = _abilityData;
    }

    public override bool CanPurchase()
    {
        if (base.CanPurchase() == false)
        {
            return false;
        }
        
        if (shopUser.ChampionAbilityManager.CheckAbility(abilityData))
        {
            Debug.LogWarning($"Ability {abilityData.AbilityName} is already owned by the user.");
            return false;
        }

        return true;
    }

    protected override void SubmitPurchaseRequest()
    {
        //shopUser.ShopPurchaseManager.TryPurchaseAbility(abilityData.AbilityID)
    }
}
*/