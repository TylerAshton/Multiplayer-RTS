using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This script is in charge of spawning the shop for the Owner and 
/// displaying it when requested provided conditions are met
/// </summary>
public class ShopDisplayManager : NetworkBehaviour
{
    [SerializeField] private GameObject championShopPrefab;

    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        if (championShopPrefab == null)
        {
            Debug.LogError($"{nameof(championShopPrefab)} is null in gameobject: {gameObject.name}");
            return;
        }

        Instantiate(championShopPrefab, transform);
    }
}
