using System;
using Unity.Netcode;
using UnityEngine;

public class ShopPurchaseManager : NetworkBehaviour
{
    private IShopUser championShopUser;
    public Action OnSuccessfullPurchase;

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

        // Refresh the shop if purchase workie
        if (purchasable.ExecutePurchase(championShopUser))
        {
            SignalSuccessfullPurchaseRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SignalSuccessfullPurchaseRpc()
    {
        OnSuccessfullPurchase?.Invoke();
    }
}
