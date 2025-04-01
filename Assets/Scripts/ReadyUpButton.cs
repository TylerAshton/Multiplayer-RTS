using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUpButton : MonoBehaviour
{
    [SerializeField] private Button readyButton; // The button the player uses to "Ready up" to progress to the next scene
    CoopPlayerManager playerManager;

    private void Awake()
    {
        playerManager = CoopPlayerManager.Instance;
        readyButton.onClick.AddListener(() =>
        {
            ulong ID = NetworkManager.Singleton.LocalClientId;
            Debug.Log(ID);
            if (ID != 0)
            {
                NetworkObject player = NetworkManager.Singleton.ConnectedClients[ID].PlayerObject;
                Debug.Log(playerManager);
                playerManager.AddPlayer(ID, player);
                foreach (KeyValuePair<ulong, GameObject> kvp in playerManager.playerPrefabs)
                {
                    Debug.Log("************");
                    Debug.Log($"{kvp.Key} + {kvp.Value}");
                    Debug.Log("************");
                }
            }
            PlayerReadyUp.Instance.SetPlayerReady();
        });
    }

    [ServerRpc(RequireOwnership = false)]
    void addToDictionaryServerRpc(ulong ID, NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject playerObject);
        playerManager.AddPlayer(ID, playerObject);
    }
}
