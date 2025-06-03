using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

public class CharacterSetArgs
{
    public CharacterSetArgs(ulong _ID) { ID = _ID; }

    public ulong ID;

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
        onCharacterSet += uimanager.SetShopType;
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc();
    }


    public EventHandler<CharacterSetArgs> onCharacterSet;

    private void raiseCharacterSet(ulong _ID)
    {
        if (onCharacterSet != null)
        {
            onCharacterSet(this, new CharacterSetArgs(_ID));
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

    [Rpc(SendTo.Server)]
    public void ChangeChampionRpc(ulong _ID, PlayerManager.ChampionTypes type)
    {
        PlayerManager.Instance.setChampionType(_ID, type);
        Debug.Log(_ID);
        Debug.Log(PlayerManager.Instance.getChampionType(_ID));
        raiseCharacterSet(_ID);
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

        coopPlayerManager.AddPlayer(clientId, playerList[prefabId]);
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
            coopPlayerManager.AddPlayer(clientId, playerList[0]);
            ChangeChampionRpc(clientId, PlayerManager.ChampionTypes.Knight);
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
