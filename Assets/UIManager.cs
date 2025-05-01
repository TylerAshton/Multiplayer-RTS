using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (id >= 0)
                {
                    NetworkObject obj = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
                    Canvas canvas = obj.GetComponentInChildren<Canvas>(true);
                    canvas.gameObject.SetActive(true);
                    canvas.GetComponentInChildren<TextMeshProUGUI>().text = ($"CLIENT ID : {id}");
                }
            }
        }
        else
        {
            return;
        }
    }
}
