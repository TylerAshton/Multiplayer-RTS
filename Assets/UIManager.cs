using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject champUI;

    [SerializeField] GameObject clericShop;
    [SerializeField] GameObject knightShop;
    public Dictionary<ulong, int> playerShops = new Dictionary<ulong, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(champUI);
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
        foreach (KeyValuePair<ulong, int> kvp in playerShops)
        {
            Debug.Log($"{kvp.Key} ++ {kvp.Value}");
        }
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

}
