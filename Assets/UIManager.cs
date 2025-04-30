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
            ShowUIRpc();
        }
        else
        {
            return;
        }
    }

    [Rpc(SendTo.Client)]
    void ShowUIRpc()
    {
        GameObject ui = GameObject.Find("ChampionUI");
        ui.GetComponent<Text>().text = ($"CLIENT ID : {NetworkManager.Singleton.LocalClientId}");
    }
}
