using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUpButton : MonoBehaviour
{
    [SerializeField] private Button readyButton; // The button the player uses to "Ready up" to progress to the next scene

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            ulong ID = NetworkManager.Singleton.LocalClientId;
            Debug.Log("DEBUG . LOG");
            if (ID != 0)
            {
                Debug.Log("DEBUG HERE");
                NetworkObject player = NetworkManager.Singleton.ConnectedClients[ID].PlayerObject;
                CoopPlayerManager.Instance.AddPlayer(ID, player.gameObject);
            }
            PlayerReadyUp.Instance.SetPlayerReady();
        });
    }
}
