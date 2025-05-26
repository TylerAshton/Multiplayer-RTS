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
        if (NetworkManager.Singleton.IsClient)
        {
            foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (id >= 0)
                {

                }
            }
        }
        else
        {
            return;
        }
    }

    [Rpc(SendTo.NotServer)]
    void enableUIRpc(ulong id)
    {
        GameObject UI = GameObject.Find("Champion UI");
        UI.gameObject.SetActive(true);
        //UI.GetComponentInChildren<TextMeshProUGUI>().text = ($"CLIENT ID : {id}");
    }
}
