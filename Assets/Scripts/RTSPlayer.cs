using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(RTSPlayerControls), typeof(UnitManager),  typeof(PlayerInput))]
public class RTSPlayer : NetworkBehaviour
{
    public static RTSPlayer Instance { get; private set; }
    private RTSPlayerControls rtsPlayerControls;
    public RTSPlayerControls RTSPlayerControls => rtsPlayerControls;
    private UnitManager unitManager;
    public UnitManager UnitManager => unitManager;
    private PlayerInput playerInput;

    private NetworkObject networkObject; 
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        networkObject = GetComponent<NetworkObject>();

        if (networkObject.IsOwner) // TODO: Refactor with try get comps
        {
            rtsPlayerControls = GetComponent<RTSPlayerControls>();
            rtsPlayerControls.Init();
            unitManager = GetComponent<UnitManager>();
            UnitManager.Init();
            playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = true;
        }
        

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
