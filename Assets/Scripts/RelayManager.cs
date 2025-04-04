using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates the host and the clients for the game
/// </summary>

public class RelayManager : NetworkBehaviour
{
    public static RelayManager Instance; //Self explanatory

    [SerializeField] Button hostButton; //Button to start the host
    [SerializeField] Button joinButton; //Button to join the host via the join code
    [SerializeField] TMP_InputField joinInput; //Text input to input the join code
    [SerializeField] TextMeshProUGUI codeText; //Displays the generated lobby code
    [SerializeField] RectTransform mainMenu; //A group of UI elements for the main menu

    [SerializeField] RectTransform characterMenu; // A group of UI elements for the player to select a character from

    [SerializeField] RectTransform readyUpMenu; // A group of UI elements for the player to use to ready up for the main game
    [SerializeField] PlayerSpawner spawner; // Instance of player spawner script

    [Header("DEBUG Only")]

    [Tooltip("Editor only: Forces the ready up button to appear even if there is only 1 player")]
    [SerializeField] private bool DEBUGIsSinglePlayer = false; 
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

    async void Start()
    {
        await UnityServices.InitializeAsync();
        
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(joinInput.text));
    }
    /// <summary>
    /// Starts Host and Internal Client as well as generates a join code for other clients
    /// </summary>
    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = "Code: " + joinCode;

        var relayServerData = allocation.ToRelayServerData("dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        joinButton.gameObject.SetActive(false);
        joinInput.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        codeText.gameObject.SetActive(true);
        //readyUpMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// Connects the External Client to an existing Host via a join code
    /// </summary>
    /// <param name="joinCode"></param>
    async void JoinRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = joinAllocation.ToRelayServerData("dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        //CoopPlayerManager.Instance.AddPlayer(AuthenticationService.Instance.PlayerId, CoopPlayer);

        mainMenu.gameObject.SetActive(false);
        characterMenu.gameObject.SetActive(true);
    }


    private void Update()
    {
        if (IsServer) { RunServerRPCs(); }
        if (IsClient) { RunClientRPCs(); }
    }

    private void RunServerRPCs()
    {
        #if UNITY_EDITOR
            if (DEBUGIsSinglePlayer)
            {
                ShowReadyUpClientRpc();
            }
        #endif
        if (NetworkManager.Singleton.ConnectedClients.Count > 1)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                ShowReadyUpClientRpc();
            }
        }
    }

    private void RunClientRPCs()
    {

    }


    [ClientRpc(RequireOwnership = false)]
    private void ShowReadyUpClientRpc()
    {
        Debug.Log("HERE!");
        readyUpMenu.gameObject.SetActive(true);
    }
}
