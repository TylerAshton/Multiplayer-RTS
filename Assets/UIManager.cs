using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameObject clericShop;
    [SerializeField] GameObject knightShop;
    public Dictionary<ulong, int> playerShops = new Dictionary<ulong, int>();

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"{playerShops[0]} , {playerShops[1]}");
    }

    public void ToggleUI()
    {
        GameObject shopObj;
        ulong id = NetworkManager.Singleton.LocalClientId;
        //Check what the type of champion is
        if (playerShops[id] == 1)
        {
            shopObj = clericShop;
        }
        else
        {
            shopObj = knightShop;
        }
        //Then asign the shop game object to match the player type
        //Once done the game will then toggle the respective ui on
        shopObj.SetActive(!shopObj.activeInHierarchy);
    }

    [Rpc(SendTo.NotServer)]
    void enableUIRpc(ulong id)
    {
        GameObject UI = GameObject.Find("Champion UI");
        UI.gameObject.SetActive(true);
        //UI.GetComponentInChildren<TextMeshProUGUI>().text = ($"CLIENT ID : {id}");
    }
}
