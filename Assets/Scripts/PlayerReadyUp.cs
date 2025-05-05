using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerReadyUp : NetworkBehaviour
{

    public static PlayerReadyUp Instance {  get; private set; }

    private Dictionary<ulong, bool> playerReadyDictionary;

    [Header("DEBUG Only")]

    [Tooltip("Editor only: Toggles the match to instead use scene Tyler as opposed to MainWorld")]
    [SerializeField] private bool DEBUGUseSceneTyler = false;

    [Tooltip("Editor only: Toggles the match to instead use scene OldMainWorld as opposed to MainWorld")]
    [SerializeField] private bool DEBUGUseSceneOld = false;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            #if UNITY_EDITOR
                if (DEBUGUseSceneTyler)
                {
                    Loader.LoadNetwork(Loader.Scene.Tyler);
                    return;
                }
                if (DEBUGUseSceneOld)
                {
                    Loader.LoadNetwork(Loader.Scene.MainWorldOLD);
                    return;
                }
            #endif
            Loader.LoadNetwork(Loader.Scene.MainWorld);
        }
    }
}
