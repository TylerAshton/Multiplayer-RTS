using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerSpawner))]
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    [SerializeField] private Transform AmalgamSpawnPos;
    [SerializeField] private GameObject AmalgamPlayer;
    [SerializeField] private Transform ChampionSpawnPos;
    [SerializeField] private GameObject ChampionPlayer;
    private PlayerSpawner playerSpawner;
    private NetworkObject networkObject;

    CoopPlayerManager playerManager;
    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerSpawner = GetComponent<PlayerSpawner>();
        playerManager = CoopPlayerManager.Instance;

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SpawnAllPlayers;
    }

    private void SpawnAllPlayers(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= SpawnAllPlayers;
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        // Spawn Players
        foreach (ulong id in GetAllConnectedClients())
        {
            GameObject newPlayer;

            if (id == 0)
            {
                newPlayer = (GameObject)Instantiate(AmalgamPlayer, AmalgamSpawnPos.position, Quaternion.identity);
            }
            else
            {
                
                newPlayer = (GameObject)Instantiate(playerManager.playerPrefabs[id], ChampionSpawnPos.position, Quaternion.identity);
                Debug.Log("HERE!");
            }

            NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
            newPlayer.SetActive(true);
            netObj.SpawnAsPlayerObject(id, true);

        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("inactive");
        }
        Debug.Log($"{NetworkManager.Singleton.IsServer} {NetworkManager.Singleton.IsClient} {NetworkManager.Singleton.IsHost}");
        if (!IsHost)
        {
            return;
        }
    }


    public static List<ulong> GetAllConnectedClients()
    {
        List<ulong> clients = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
        return clients;
    }

    private void SpawnAmalgam()
    {

    }
}
