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
    public void HandlePurchaseRequestRpc(string _purchaseID)
    {
        Purchasable purchasable = Registry<Purchasable>.GetItem(_purchaseID);

        purchasable.ExecutePurchase(championShopUser);
    }
}
