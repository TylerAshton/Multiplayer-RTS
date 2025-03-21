using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] List<GameObject> playerList;
    [SerializeField] GameObject CoopPlayerPrefab;
    GameObject CoopPlayer;
    [SerializeField] GameObject RTSPlayer;
    public List<GameObject> CoopPlayerPrefabList;
    private int prefabNumber;

    async public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, prefabNumber);
        }
    }

    public void changePrefab(int prefabId)
    {
        prefabNumber = prefabId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        Debug.Log(prefabId);
        GameObject newPlayer;

        newPlayer = (GameObject)Instantiate(CoopPlayerPrefabList[prefabId]);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
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
}
