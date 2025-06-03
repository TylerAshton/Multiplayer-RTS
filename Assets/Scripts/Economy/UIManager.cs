using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    public Shop currentShop;

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

    public void SetShopType(object sender, CharacterSetArgs args)
    {
        if (PlayerManager.Instance.getChampionType(args.ID) == PlayerManager.ChampionTypes.Knight)
        {
            currentShop = PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<KnightShop>();
        }
        else if (PlayerManager.Instance.getChampionType(args.ID) == PlayerManager.ChampionTypes.Cleric)
        {
            currentShop = PlayerManager.Instance.getPlayerGameObject(args.ID).GetComponentInChildren<ClericShop>();
        }
    }
}
