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

    [Header("Debug Only")]
    [SerializeField] private bool DEBUGForceChampion = false;


    

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

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SpawnAllPlayers;
    }

    /// <summary>
    /// Ran once level loading is complete, spawning all the player prefabs into the level by clientID
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="sceneName"></param>
    /// <param name="loadSceneMode"></param>
    private void SpawnAllPlayers(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) // TODO: Remove args
                                                                                                // TODO: Use player dict
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

            if (id == 0) // RTS
            {
                #if UNITY_EDITOR
                    if (DEBUGForceChampion)
                    {
                        newPlayer = (GameObject)Instantiate(ChampionPlayer, ChampionSpawnPos.position, Quaternion.identity);
                    }
                    else // I'm important leave me alone
                #endif
                {
                    newPlayer = (GameObject)Instantiate(AmalgamPlayer, AmalgamSpawnPos.position, Quaternion.identity);
                }
            }

            else // COOP
            {
                newPlayer = (GameObject)Instantiate(ChampionPlayer, ChampionSpawnPos.position, Quaternion.identity);
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

    /// <summary>
    /// Returns all the client IDs
    /// </summary>
    /// <returns></returns>
    public static List<ulong> GetAllConnectedClients()
    {
        List<ulong> clients = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
        return clients;
    }
}
