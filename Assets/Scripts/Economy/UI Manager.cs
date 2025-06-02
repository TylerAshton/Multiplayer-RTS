using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject clericUI;
    [SerializeField] private GameObject knightUI;

    Shop currentShop;

    public bool inShopZone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //currentShop => KnightShop
        //currentShop => ClericShop
    }

    public void SetCurrentShop(object eventSender, ShopPopulatedArgs shopEventArgs)
    {
        if (shopEventArgs.playerShop == 0)
        {
            currentShop = Instantiate(clericUI).GetComponent<ClericShop>();
        }
        else if (shopEventArgs.playerShop == 1)
        {
            currentShop = Instantiate(clericUI).GetComponent<KnightShop>();
        }
    }

    public void ToggleUI()
    {
        Debug.Log("PISS OFF");
    }
}
