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

    private void Awake()
    {
        playerManager = CoopPlayerManager.Instance;
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void changePrefab(int prefabId)
    {
        //tempPosition = await getClientTransform(NetworkManager.Singleton.LocalClientId);
        DespawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, prefabId, tempPosition);
    }

    private Vector3 getClientTransform(ulong clientId)
    {
        NetworkObject tempPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        tempPosition = tempPlayer.transform.position;
        return tempPosition;
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
        Debug.Log(prefabId);
        GameObject newPlayer;

        newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);

        //playerManager.AddPlayer(clientId, playerList[prefabId]);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject newPlayer;

        if (clientId == 0)
        {
            newPlayer = (GameObject)Instantiate(RTSPlayer);
        }
        else
        {
            newPlayer = (GameObject)Instantiate(CoopPlayerPrefab);
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientId, int prefabId, Vector3 position)
    {
        Debug.Log(prefabId);
        GameObject newPlayer;

        newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
