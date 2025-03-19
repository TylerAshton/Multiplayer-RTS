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

/// <summary>
/// Creates the host and the clients for the game
/// </summary>

public class RelayManager : NetworkBehaviour
{
    public static RelayManager Instance;

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TextMeshProUGUI codeText;
    [SerializeField] Canvas canvas;
    [SerializeField] List<GameObject> playerList;
    [SerializeField] GameObject CoopPlayerPrefab;
    GameObject CoopPlayer;
    [SerializeField] GameObject RTSPlayer;


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
    /// Starts host and Internal Client
    /// </summary>
    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        codeText.text = "Code: " + joinCode;

        var relayServerData = allocation.ToRelayServerData("dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        //CoopPlayer = Instantiate(RTSPlayer);

        joinButton.gameObject.SetActive(false);
        joinInput.gameObject.SetActive(false);
    }

    /// <summary>
    /// External Client
    /// </summary>
    /// <param name="joinCode"></param>
    async void JoinRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = joinAllocation.ToRelayServerData("dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        //Debug.Log(AuthenticationService.Instance.PlayerId);

        //CoopPlayer = Instantiate(CoopPlayerPrefab);

        canvas.gameObject.SetActive(false);
    }


    /// <summary>
    /// Command for the server to create a player and spawn it in.
    /// </summary>
    //[Rpc(SendTo.Server)]
    public void CreatePlayerRpc()
    {
        Debug.Log($"{NetworkManager.Singleton.IsHost}, {NetworkManager.Singleton.IsClient}, {NetworkManager.Singleton.IsServer}");
        CoopPlayer.GetComponent<NetworkObject>().Spawn();
        CoopPlayerManager.Instance.AddPlayer(AuthenticationService.Instance.PlayerId, CoopPlayer);
    }
}
