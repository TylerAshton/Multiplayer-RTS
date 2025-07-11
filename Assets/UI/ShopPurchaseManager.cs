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
        if (ability == null)
        {
            Debug.LogError($"Ability with ID {abilityID} not found in registry.");
            return;
        }

        int playerPoints = PointManager.Instance.GetPoints(championShopUser.PlayerID);
        if (playerPoints < ability.PurchasePrice)
        {
            Debug.LogError($"Player {OwnerClientId} insufficient points for ability {abilityID}");
            return;
        }


        championShopUser.ChampionAbilityManager.AddAbility(ability, 0);
        PointManager.Instance.RemovePoints(championShopUser.PlayerID, ability.PurchasePrice);
    }

    [Rpc(SendTo.Server)]
    public void HandleHealPurchaseRequestRpc(int healthCost, int healthAmount)
    {
        if (healthCost > PointManager.Instance.GetPoints(championShopUser.PlayerID))
        {
            Debug.LogError($"Player {OwnerClientId} insufficient points for healing. Required: {healthCost}, Available: {PointManager.Instance.GetPoints(championShopUser.PlayerID)}");
            return;
        }

        championShopUser.ChampionHealth.Heal(healthAmount);
        PointManager.Instance.RemovePoints(championShopUser.PlayerID, healthCost);
    }
}
