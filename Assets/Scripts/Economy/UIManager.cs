using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    private Dictionary<PlayerManager.ChampionTypes, Shop> IDtoUI = new Dictionary<PlayerManager.ChampionTypes, Shop>();
    private Dictionary<ulong, bool> PlayerInShop = new Dictionary<ulong, bool>();

    public Shop currentShop;

    private NetworkObject networkObject;

    public bool inShopZone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        Debug.Log(networkObject.NetworkObjectId);
    }

    public bool getPlayerInShop(ulong _ID)
    {
        return PlayerInShop[_ID];
    }

    public void setPlayerInShop(ulong _ID, bool _InShop)
    {
        setPlayerInShopRpc(_ID, _InShop);
    }

    [Rpc(SendTo.Everyone)]
    private void setPlayerInShopRpc(ulong _ID, bool _InShop)
    {
        try
        {
            PlayerInShop.Add(_ID, _InShop);
            PlayerManager.Instance.getPlayerGameObject(_ID).GetComponent<ChampionManager>().inShop = _InShop;
        }
        catch (ArgumentException)
        {
            PlayerInShop.Remove(_ID);
            PlayerInShop.Add(_ID, _InShop);
            PlayerManager.Instance.getPlayerGameObject(_ID).GetComponent<ChampionManager>().inShop = _InShop;
        }
    }

    private void setUI(PlayerManager.ChampionTypes _type, Shop _ShopObject)
    {
        try
        {
            IDtoUI.Add(_type, _ShopObject);
        }
        catch (ArgumentException)
        {
            IDtoUI.Remove(_type);
            IDtoUI.Add(_type, _ShopObject);
        }
    }


    public void SetShopType(object sender, CharacterSetArgs args)
    {
        Debug.Log(args.type);
        if (args.type == PlayerManager.ChampionTypes.Knight)
        {
            //currentShop = PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<KnightShop>();
            setUI(args.type, PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<KnightShop>());
        }
        else if (args.type == PlayerManager.ChampionTypes.Cleric)
        {
            //currentShop = PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<ClericShop>();
            setUI(args.type, PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<ClericShop>());
        }
        Debug.Log(args.type);
    }
}
