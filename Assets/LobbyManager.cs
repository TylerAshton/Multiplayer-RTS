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
    public Transform PChampionSpawnPos => ChampionSpawnPos;
    [SerializeField] private GameObject ChampionPlayer;
    private PlayerSpawner playerSpawner;
    private NetworkObject networkObject;
    [SerializeField] private WinManager winManager;

    [Header("Debug Only")]

    [Tooltip("Editor only: Replaces RTS spawning with Champion spawning instead.")]
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
            Debug.LogError($"{nameof(NetworkObject)} is required for {GetType().Name}");
            return;
        }
        if (winManager == null)
        {
            Debug.LogError($"{nameof(WinManager)} is required for {GetType().Name}");
            return;
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
        if (!NetworkManager.Singleton.IsHost)
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
                newPlayer = (GameObject)Instantiate(PlayerManager.Instance.getPlayerGameObject(id), ChampionSpawnPos.position, Quaternion.identity);
                winManager.SelectChampion(newPlayer.GetComponent<Health>()); // Register the player as a champion to winManager
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
