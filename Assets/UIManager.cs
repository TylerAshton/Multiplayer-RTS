using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject champUI;

    [SerializeField] GameObject clericShop;
    [SerializeField] GameObject knightShop;
    private Dictionary<ulong, int> playerShops = new Dictionary<ulong, int>();

    [SerializeField] public bool inShopZone = false;

    private GameObject shopObj;

    public Vector2 moveInput;

    public TextMeshProUGUI[] optionsCleric;
    public TextMeshProUGUI[] optionsKnight;

    public TextMeshProUGUI[] options;

    public Color normalColour, highlightedColour;

    private NetworkObject networkObject;

    int selectedOption;

    private NetworkObject player;

    [SerializeField] private List<Ability> abilitesCleric;
    [SerializeField] private List<Ability> abilitesKnight;

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
        networkObject = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shopObj.activeInHierarchy)
        {
            moveInput.x = Input.mousePosition.x - (Screen.width / 2f);
            moveInput.y = Input.mousePosition.y - (Screen.height / 2f);
            moveInput.Normalize();
            if (moveInput != Vector2.zero)
            {
                float angle = Mathf.Atan2(moveInput.y, -moveInput.x) / Mathf.PI;
                angle = angle * 180;
                angle += 90f; //Rotate it so 0 degrees is at the bottom
                if (angle < 0)
                {
                    angle += 360;
                }

                player = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;

                if (playerShops[NetworkManager.Singleton.LocalClientId] == 0)
                {
                    options = optionsKnight;
                }
                else
                {
                    options = optionsCleric;
                }

                for (int i = 0; i < options.Length; i++)
                {
                    if (angle > i * 180 && angle < (i + 1) * 180)
                    {
                        options[i].color = highlightedColour;
                        selectedOption = i;
                    }
                    else
                    {
                        options[i].color = normalColour;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                switch (selectedOption)
                {
                    case 0:
                        player.GetComponent<Health>().Heal(999);
                        break;
                    case 1:
                        Debug.Log(player.GetComponent<ChampionAbilityManager>().abilities.Contains(abilitesKnight[selectedOption - 1]));
                        if (playerShops[NetworkManager.Singleton.LocalClientId] == 1)
                        {
                            if (!player.GetComponent<ChampionAbilityManager>().abilities.Contains(abilitesCleric[selectedOption - 1]))
                            {
                                Debug.Log("AWARDED TO CLERIC");
                                player.GetComponent<ChampionAbilityManager>().abilities.Add(abilitesCleric[selectedOption - 1]);
                            }
                        }
                        else
                        {
                            if (!player.GetComponent<ChampionAbilityManager>().abilities.Contains(abilitesKnight[selectedOption - 1]))
                            {
                                Debug.Log("AWARDED TO KNIGHT");
                                player.GetComponent<ChampionAbilityManager>().abilities.Add(abilitesKnight[selectedOption - 1]);
                            }
                        }
                        break;
                }

                ToggleUI();
            }
        }
    }

    public void AddtoShop(ulong _ClientID, int _PrefabID)
    {
        AddRpc(_ClientID, _PrefabID);
    }

    public void RemovefromShop(ulong _ClientID, int _PrefabID)
    {
        RemoveRpc(_ClientID, _PrefabID);
    }

    [Rpc(SendTo.Everyone)]
    void AddRpc(ulong _ClientID, int _PrefabID)
    {
        try
        {
            playerShops.Add(_ClientID, _PrefabID);
        }
        catch (ArgumentException)
        {
            playerShops.Remove(_ClientID);
            playerShops.Add(_ClientID, _PrefabID);
        }
    }

    [Rpc(SendTo.Everyone)]
    void RemoveRpc(ulong _ClientID, int _PrefabID)
    {
        playerShops.Remove(_ClientID);
    }
    public void ToggleUI()
    {
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
        if (!inShopZone) { return; }
        else
        {
            shopObj.SetActive(!shopObj.activeInHierarchy);
        }
    }
}
