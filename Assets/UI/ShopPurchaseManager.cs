using System;
using Unity.Netcode;
using UnityEngine;

public class ShopPurchaseManager : NetworkBehaviour
{
    private IShopUser championShopUser;

    private void Awake()
    {
        if (!TryGetComponent<IShopUser>(out championShopUser))
        {
            Debug.LogError($"{GetType().Name} requires {nameof(IShopUser)} within gameobject: {gameObject.name}!");
            return;
        }
    }

    [Rpc(SendTo.Server)]
    public void HandleAbilityPurchaseRequestRpc(string abilityID)
    {
        Ability ability = AbilityRegistry.GetAbility(abilityID);
        championShopUser.ChampionAbilityManager.AddAbility(ability, 0);
        PointManager.Instance.RemovePoints(championShopUser.PlayerID, ability.PurchasePrice);
    }

    [Rpc(SendTo.Server)]
    public void HandleHealPurchaseRequestRpc(int healthCost, int healthAmount)
    {
        championShopUser.ChampionHealth.Heal(healthAmount);
    }
}
