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

        if (networkObject.IsOwner)
        {
            if (!TryGetComponent<RTSPlayerControls>(out rtsPlayerControls))
            {
                Debug.LogError($"{nameof(RTSPlayerControls)} is required for {GetType().Name}");
                return;
            }
            rtsPlayerControls.Init();
            if (!TryGetComponent<UnitManager>(out unitManager))
            {
                Debug.LogError($"{nameof(UnitManager)} is required for {GetType().Name}");
                return;
            }
            UnitManager.Init();
            if (!TryGetComponent<PlayerInput>(out playerInput))
            {
                Debug.LogError($"{nameof(PlayerInput)} is required for {GetType().Name}");
                return;
            }
            playerInput.enabled = true;
        }
    }
}
