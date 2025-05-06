using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] List<GameObject> playerList; // THIS IS THE ACTUAL PREFABS USED IN GAME
    [SerializeField] GameObject CoopPlayerPrefab;
    GameObject CoopPlayer;
    [SerializeField] GameObject RTSPlayer;
    public List<GameObject> CoopPlayerPrefabList; // THIS IS THE MENU PREFABS
    private int prefabNumber;
    private Vector3 tempPosition = new(0,0,0);

    CoopPlayerManager playerManager;
    UIManager uimanager;

    private void Awake()
    {
        playerManager = CoopPlayerManager.Instance;
        uimanager = UIManager.Instance;
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc();
    }

    public void changePrefab(int prefabId)
    {
        //tempPosition = await getClientTransform(NetworkManager.Singleton.LocalClientId);
        DespawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, prefabId);
        AddShop(NetworkManager.Singleton.LocalClientId, prefabId);
    }

    public void AddShop(ulong _ID, int _Prefab)
    {
        try
        {
            uimanager.playerShops.Add(_ID, _Prefab);
        }
        catch (ArgumentException)
        {
            uimanager.playerShops.Remove(_ID);
            uimanager.playerShops.Add(_ID, _Prefab);
        }
    }

    //private Vector3 getClientTransform(ulong clientId)
    //{
    //    NetworkObject tempPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
    //    tempPosition = tempPlayer.transform.position;
    //    return tempPosition;
    //}

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
        Debug.Log(prefabId);
        GameObject newPlayer;

        newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);

        playerManager.AddPlayer(clientId, playerList[prefabId]);
        Debug.Log(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        GameObject newPlayer;
        
        if (clientId == 0)
        {
            newPlayer = (GameObject)Instantiate(RTSPlayer);
            AddShop(clientId, -1);
        }
        else
        {
            newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[0]);
            playerManager.AddPlayer(clientId, playerList[0]);
            AddShop(clientId, 0);
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnPlayerServerRpc(ulong clientId, int prefabId, Vector3 position)
    //{
    //    Debug.Log(prefabId);
    //    GameObject newPlayer;

    //    newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

    //    NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
    //    newPlayer.SetActive(true);
    //    netObj.SpawnAsPlayerObject(clientId, true);
    //    playerManager.AddPlayer(clientId, playerList[prefabId]);
    //}

    private void setPlayerPrefabGlobal(ulong _ID, GameObject _Prefab)
    {
        playerManager.AddPlayer(_ID, _Prefab);
    }
}
