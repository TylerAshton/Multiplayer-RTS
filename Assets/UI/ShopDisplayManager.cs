using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This script is in charge of spawning the shop for the Owner and 
/// displaying it when requested provided conditions are met
/// </summary>
public class ShopDisplayManager : NetworkBehaviour
{
    [SerializeField] private GameObject championShopPrefab;
    private UIDocument shopUIDocument;

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

        GameObject championShop = Instantiate(championShopPrefab, transform);
        if (!championShop.TryGetComponent<UIDocument>(out shopUIDocument))
        {
            Debug.LogError($"{nameof(shopUIDocument)} was not found in the {nameof(championShopPrefab)} prefab!");
            return;
        }

        // Disable the shop UI initially
        shopUIDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void CloseShopUI()
    {
        Debug.Log("Closing Shop");
        shopUIDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void ToggleShopUI()
    {
        shopUIDocument.rootVisualElement.style.display =
            shopUIDocument.rootVisualElement.style.display == DisplayStyle.None ?
            DisplayStyle.Flex : DisplayStyle.None;
    }
}
