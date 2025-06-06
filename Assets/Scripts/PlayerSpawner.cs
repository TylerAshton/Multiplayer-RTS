using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSetArgs
{
    public CharacterSetArgs(ulong _ID, PlayerManager.ChampionTypes _type) { ID = _ID; type = _type; }

    public ulong ID;
    public PlayerManager.ChampionTypes type;

}

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] List<GameObject> playerList; // THIS IS THE ACTUAL PREFABS USED IN GAME
    [SerializeField] GameObject CoopPlayerPrefab;
    GameObject CoopPlayer;
    [SerializeField] GameObject RTSPlayer;
    public List<GameObject> CoopPlayerPrefabList; // THIS IS THE MENU PREFABS
    private int prefabNumber;
    private Vector3 tempPosition = new(0, 0, 0);

    CoopPlayerManager coopPlayerManager;
    UIManager uimanager;
    PlayerManager playerManager;

    private void Awake()
    {
        uimanager = UIManager.Instance;
        coopPlayerManager = CoopPlayerManager.Instance;
        playerManager = PlayerManager.Instance;
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc();
    }


    public EventHandler<CharacterSetArgs> onCharacterSet;

    private void raiseCharacterSet(ulong _ID, PlayerManager.ChampionTypes _type)
    {
        if (onCharacterSet != null)
        {
            onCharacterSet(this, new CharacterSetArgs(_ID, _type));
        }
    }

    
    private PlayerManager.ChampionTypes convertIndextoType(int _Index)
    {
        if (_Index == 0)
        {
            return PlayerManager.ChampionTypes.Cleric;
        }
        else
        {
            return PlayerManager.ChampionTypes.Knight;
        }
    }

    public void changePrefab(int _PrefabID)
    {
        DespawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, _PrefabID);
        ChangeChampionRpc(NetworkManager.Singleton.LocalClientId, convertIndextoType(_PrefabID));
    }

    [Rpc(SendTo.Everyone)]
    public void ChangeChampionRpc(ulong _ID, PlayerManager.ChampionTypes type)
    {
        int index;
        PlayerManager.Instance.setChampionType(_ID, type);
        if (type == PlayerManager.ChampionTypes.Cleric)
        {
            index = 0;
        }
        else
        {
            index = 1;
        }
        PlayerManager.Instance.setPlayerGameObject(_ID, playerList[index]);
        //raiseCharacterSet(_ID, type);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnPlayerServerRpc(ulong clientId)
    {
        NetworkObject tempPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        tempPosition = tempPlayer.transform.position;
        Destroy(tempPlayer.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        GameObject newPlayer;

        newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);

        playerManager.setPlayerGameObject(clientId, playerList[prefabId]);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        GameObject newPlayer;

        if (clientId == 0)
        {
            newPlayer = (GameObject)Instantiate(RTSPlayer);
        }
        else
        {
            newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[0]);
            playerManager.setPlayerGameObject(clientId, playerList[0]);
            ChangeChampionRpc(clientId, PlayerManager.ChampionTypes.Cleric);
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
