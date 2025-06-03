using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public enum ChampionTypes
    {
        Knight,
        Cleric
    }

    private Dictionary<ulong, ChampionTypes> IDtoChampion = new Dictionary<ulong, ChampionTypes>();
    private Dictionary<ulong, GameObject> IDtoGameObject = new Dictionary<ulong, GameObject>();

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

    public ChampionTypes getChampionType(ulong _ID)
    {
        return IDtoChampion[_ID];
    }

    public void setChampionType(ulong _ID, ChampionTypes _ChampionType)
    {
        try
        {
            IDtoChampion.Add(_ID, _ChampionType);
        }
        catch (ArgumentException)
        {
            IDtoChampion.Remove(_ID);
            IDtoChampion.Add(_ID,_ChampionType);
        }
    }

    public GameObject getPlayerGameObject(ulong _ID)
    {
        return IDtoGameObject[_ID];
    }

    public void setPlayerGameObject(ulong _ID, GameObject _PlayerObject)
    {
        try
        {
            IDtoGameObject.Add(_ID, _PlayerObject);
        }
        catch (ArgumentException)
        {
            IDtoGameObject.Remove(_ID);
            IDtoGameObject.Add(_ID, _PlayerObject);
        }
    }

    public void setPlayerGameObject(ulong _ID, NetworkObject _NetworkObject)
    {
        setPlayerGameObject(_ID, _NetworkObject.gameObject);
    }
}
