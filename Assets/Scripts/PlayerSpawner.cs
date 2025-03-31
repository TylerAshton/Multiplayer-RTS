using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] List<GameObject> playerList; //
    [SerializeField] GameObject CoopPlayerPrefab;
    GameObject CoopPlayer;
    [SerializeField] GameObject RTSPlayer;
    public List<GameObject> CoopPlayerPrefabList;
    private int prefabNumber;
    private Vector3 tempPosition = new(0,0,0);

    CoopPlayerManager playerManager;

    private void Start()
    {
        playerManager = CoopPlayerManager.Instance;
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc();
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

    private void setPlayerPrefabGlobal(ulong _ID, GameObject _Prefab)
    {
        playerManager.AddPlayer(_ID, _Prefab);
    }
}
