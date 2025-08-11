using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChampionControls : NetworkBehaviour
{
    private PlayerInput playerInput;
    private Vector2 mouseScreenPos = Vector3.zero;
    public Vector2 MouseScreenPos => mouseScreenPos;
    private NetworkObject networkObject;

    private ChampionMovement champMove;

    [Header("Ability")]
    private ChampionAbilityManager championAbilityManager;

    [Header("Mouse Aiming")]
    [SerializeField] private float aimPositionUpdateTolerance = 0.1f;
    public Vector3 AimPoint => aimPoint;
    private Vector3 aimPoint;
    private Vector3 worldPosition; // the position of the mouse relative to the world origin
    public Vector3 WorldPosition => worldPosition;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask environmentMask; // phyiscal stuff
    [SerializeField] private LayerMask characterMask; // Characters and enemies 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<ChampionMovement>(out champMove))
        {
            Debug.LogError($"{nameof(ChampionMovement)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!networkObject.IsOwner) { return; }
        if (!TryGetComponent<PlayerInput>(out playerInput))
        {
            Debug.LogError($"{nameof(PlayerInput)} is required for {GetType().Name} on gameobject {gameObject.name}!");
        }
        playerInput.enabled = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void TryApplyAimPosition()
    {
        if (!IsOwner) { return; }
        Vector3 newAimPosition = GetAimPosition();

        if (newAimPosition != aimPoint && Vector3.Distance(newAimPosition, AimPoint) >= aimPositionUpdateTolerance)
        {
            aimPoint = newAimPosition;
            ApplyAimPositionRpc(newAimPosition);
        }
    }

    /// <summary>
    /// Raycasts to the mousePosition and returns the center Position of the hit object if it is an enemy or player. Otherwise returns the worldPosition with the y coordinate set to the player's y coordinate.
    /// </summary>
    /// <returns></returns>
    private Vector3 GetAimPosition()
    {
        if (!IsOwner)
        {
            Debug.LogError($"{nameof(GetAimPosition)} called on non-owner client in gameobject: {gameObject.name}!");
            return aimPoint;
        }

        Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);

        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, characterMask))
        {
            if (hit.collider.gameObject != gameObject && hit.collider.CompareTag("Amalgam") || hit.collider.CompareTag("Champion"))
            {
                return hit.collider.bounds.center;
            }
        }

        return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
    }

    [Rpc(SendTo.Server)]
    private void ApplyAimPositionRpc(Vector3 _newAimPosition)
    {
        aimPoint = _newAimPosition;
    }

    /// <summary>
    /// This is called all the time to aquire screen position and update the mouseScreenPos variable
    /// </summary>
    /// <param name="context"></param>
    public void OnPoint(InputAction.CallbackContext context)
    {
        mouseScreenPos = context.ReadValue<Vector2>();
        worldPosition = new Vector3(0, 0, 0);

        Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);
        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, environmentMask))
        {
            worldPosition = hit.point;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ServerUpdate();
        OwnerUpdate();
    }

    /// <summary>
    /// Runs all update logic for the server
    /// </summary>
    private void ServerUpdate()
    {
        if (!IsServer) { return; }
    }

    /// <summary>
    /// Runs all update logic for the client who owns the champion
    /// </summary>
    private void OwnerUpdate()
    {
        if (!IsOwner) { return; }
        TryApplyAimPosition();
    }

    /// <summary>
    /// Casts the ability relevant to the parsed index. By calling the Ability's Activate() function
    /// </summary>
    /// <param name="_AbilityIndex"></param>
    [ServerRpc(RequireOwnership = false)]
    private void CastAbilityServerRpc(int _AbilityIndex) // TODO: Should really be moved into abilityManager or something
    {
        championAbilityManager.TryCastAbility(_AbilityIndex, 0);
    }

    /// <summary>
    /// Calls the server to use the primary ability the champion has
    /// </summary>
    /// <param name="context"></param>
    public void UsePrimaryAbility(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;
        CastAbilityServerRpc(0);
    }

    /// <summary>
    /// Casts the units secondary ability in tab 0 if it exists.
    /// </summary>
    /// <param name="context"></param>
    public void UseSecondaryAbility(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (!context.performed) return;

        if (championAbilityManager.AbilityTabs[0].Abilities.Count < 2)
        {
            Debug.LogWarning("No secondary ability available.");
            return;
        }

        CastAbilityServerRpc(1);
    }

    /// <summary>
    /// Casts the units secondary ability in tab 0 if it exists.
    /// </summary>
    /// <param name="context"></param>
    public void Use3rdAbility(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (!context.performed) return;

        if (championAbilityManager.AbilityTabs[0].Abilities.Count < 3)
        {
            Debug.LogWarning("No 3rd ability available.");
            return;
        }

        CastAbilityServerRpc(2);
    }

    /// <summary>
    /// Casts the units secondary ability in tab 0 if it exists.
    /// </summary>
    /// <param name="context"></param>
    public void Use4thAbility(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (!context.performed) return;

        if (championAbilityManager.AbilityTabs[0].Abilities.Count < 4)
        {
            Debug.LogWarning("No 4th ability available.");
            return;
        }

        CastAbilityServerRpc(3);
    }

    /// <summary>
    /// The unity input system uses this function to capture the input for the player movement
    /// </summary>
    /// <param name="context"></param>
    public void CheckMove(InputAction.CallbackContext context)
    {
        Vector3 newMovementVector = new Vector3();
        newMovementVector.x = context.ReadValue<Vector2>().x;
        newMovementVector.y = 0;
        newMovementVector.z = context.ReadValue<Vector2>().y;

        SetMoveInputServerRpc(newMovementVector); //----------- This will be in the movement script
    }

    [ServerRpc]
    private void SetMoveInputServerRpc(Vector3 _newMovementVector)
    {
        champMove.movementVector = _newMovementVector;
    }

    [Rpc(SendTo.Owner)]
    public void ToggleControlsRpc(bool _value)
    {
        if (!IsOwner)
        {
            Debug.LogError($"Client attempted to toggle controls on a non-owner client in gameobject: {gameObject.name}!");
            return;
        }

        playerInput.enabled = _value;
    }
}