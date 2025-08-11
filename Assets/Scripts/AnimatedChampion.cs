using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

//public interface IShopUser
//{
//    ChampionAbilityManager ChampionAbilityManager { get; }
//    Health ChampionHealth { get; }
//    int Points { get; } 
//    ShopPurchaseManager ShopPurchaseManager { get; }

//    ulong PlayerID { get; }
//}

[RequireComponent(typeof(Animator))]
public class AnimatedChampion : NetworkBehaviour, ICharacterAbilityUser, IFaction, IRevivable, IShopUser
{
    //[SerializeField] private float moveSpeed = 4f; //movement speed multiplier REDACTED DUE TO STAT-MANAGER
    [SerializeField] private Vector3 movementRotationOffset = Vector3.zero;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    private RelayManager manager; //relay manager instance
    private Rigidbody rb; //rigidbody attached to the player

    private Vector3 movementVector; // SERVER ONLY the movement vector to be added to the transform
    private CameraSpawner cameraSpawner; //camera spawner instance
    private NetworkObject networkObject; // current networkObject attached to the player

    private Vector3 worldPosition; // the position of the mouse relative to the world origin
    public Vector3 WorldPosition => worldPosition;

    public NetCodeAnimationManager NAnimator => nAnimator;
    private NetCodeAnimationManager nAnimator;

    private AnimationTriggerManager animTriggerManager;
    public AnimationTriggerManager AnimTriggerManager => animTriggerManager;

    public Transform Transform => transform;

    private AbilityPositionManager abilityPositionManager;

    public IReadOnlyDictionary<AbilityPosition, Transform> AbilityPositions => abilityPositionManager.AbilityPositions;

    private EffectManager effectManager;
    public EffectManager EffectManager => effectManager;

    private Faction faction = Faction.Champion;
    Faction IFaction.Faction { get => faction; set => faction = value; }

    public IFaction IFaction => this;

    private Vector3 aimPoint;
    public Vector3 AimPoint => aimPoint;

    private GameObject playerCamera; // the camera that the player will be seeing the game through

    private ChampionAbilityManager championAbilityManager;
    private CharacterController characterController;
    private PlayerInput playerInput;
    private ShopDisplayManager ShopDisplayManager;

    private Vector3 velocity; // used for gravity shit

    [SerializeField] private Ability primaryAbility;

    private UIManager uiManager;
    private PlayerManager playerManager;

    [SerializeField] private TextMeshProUGUI points;

    public bool inShop = false;
    private StatManager statManager;

    private Vector2 mouseScreenPos = Vector3.zero;
    public Vector2 MouseScreenPos => mouseScreenPos;

    public ChampionAbilityManager ChampionAbilityManager => championAbilityManager;
    public AbilityManager AbilityManager => championAbilityManager;

    public Health ChampionHealth => health;

    public int Points => PointManager.Instance.GetPoints(OwnerClientId);

    public ulong PlayerID => OwnerClientId;

    private ShopPurchaseManager shopPurchaseManager;
    public ShopPurchaseManager ShopPurchaseManager => shopPurchaseManager;

    [SerializeField] private LayerMask environmentMask; // phyiscal stuff
    [SerializeField] private LayerMask characterMask; // Characters and enemies 
    [SerializeField] private float aimPositionUpdateTolerance = 0.1f;
    [SerializeField] private GameObject soulPrefab;
    [SerializeField] private Vector3 soulSpawnOffset = Vector3.zero;

    private Health health;

    public Health Health => health;

    public ulong OwnerID => networkObject.OwnerClientId;

    void Start()
    {
        manager = RelayManager.Instance;
        uiManager = UIManager.Instance;
        playerManager = PlayerManager.Instance;
        rb = GetComponent<Rigidbody>();

        if (!TryGetComponent<AnimationTriggerManager>(out animTriggerManager))
        {
            Debug.LogError($"{nameof(AnimationTriggerManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError($"{nameof(CameraSpawner)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError($"{nameof(NetworkObject)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }

        if (!TryGetComponent<NetCodeAnimationManager>(out nAnimator))
        {
            Debug.LogError($"{nameof(NetCodeAnimationManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<EffectManager>(out effectManager))
        {
            Debug.LogError($"{nameof(EffectManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ChampionAbilityManager>(out championAbilityManager))
        {
            Debug.LogError($"{nameof(ChampionAbilityManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError($"{nameof(AbilityPositionManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<CharacterController>(out characterController))
        {
            Debug.LogError($"{nameof(CharacterController)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<StatManager>(out statManager))
        {
            Debug.LogError($"{nameof(StatManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<Health>(out health))
        {
            Debug.LogError($"{nameof(Health)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ShopDisplayManager>(out ShopDisplayManager))
        {
            Debug.LogError($"{nameof(ShopDisplayManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ShopPurchaseManager>(out shopPurchaseManager))
        {
            Debug.LogError($"{nameof(ShopPurchaseManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }


        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
            if (!TryGetComponent<PlayerInput>(out playerInput))
            {
                Debug.LogError($"{nameof(PlayerInput)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            }
            playerInput.enabled = true;

        }

        Cursor.lockState = CursorLockMode.Confined;

        MinimapHandler.Instance.updateList();
        MinimapHandler.Instance.createIcon(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(aimPoint, 0.5f); // Draw a wire sphere at the aim point for debugging
/*        Gizmos.DrawLine(abilityPositionManager.AbilityPositions[AbilityPosition.RightHand].position, aimPoint); // Draw a line from the player to the aim point*/
    }

    //private void TryApplyAimPosition()
    //{
    //    if (!IsOwner) { return; }
    //    Vector3 newAimPosition = GetAimPosition();

    //    if (newAimPosition != aimPoint && Vector3.Distance(newAimPosition, AimPoint) >= aimPositionUpdateTolerance)
    //    {
    //        aimPoint = newAimPosition;
    //        ApplyAimPositionRpc(newAimPosition);
    //    }
    //}

    ///// <summary>
    ///// Raycasts to the mousePosition and returns the center Position of the hit object if it is an enemy or player. Otherwise returns the worldPosition with the y coordinate set to the player's y coordinate.
    ///// </summary>
    ///// <returns></returns>
    //private Vector3 GetAimPosition()
    //{
    //    if (!IsOwner) 
    //    { 
    //        Debug.LogError($"{nameof(GetAimPosition)} called on non-owner client in gameobject: {gameObject.name}!");
    //        return aimPoint; 
    //    }

    //    Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);

    //    if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, characterMask))
    //    {
    //        if (hit.collider.gameObject != gameObject && hit.collider.CompareTag("Amalgam") || hit.collider.CompareTag("Champion"))
    //        {
    //            return hit.collider.bounds.center;
    //        }
    //    }

    //    return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
    //}

    //[Rpc(SendTo.Server)]
    //private void ApplyAimPositionRpc(Vector3 _newAimPosition)
    //{
    //    aimPoint = _newAimPosition;
    //}

    ///// <summary>
    ///// This is called all the time to aquire screen position and update the mouseScreenPos variable
    ///// </summary>
    ///// <param name="context"></param>
    //public void OnPoint(InputAction.CallbackContext context)
    //{
    //    mouseScreenPos = context.ReadValue<Vector2>();
    //    worldPosition = new Vector3(0, 0, 0);

    //    Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);
    //    if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, environmentMask))
    //    {
    //        worldPosition = hit.point;
    //    }
    //}

    public void AttemptToggleUI() // TODO: This should really be reworked into ShopDisplayManager
    {
        if (inShop && networkObject.IsOwner) // TODO: Move this to ShopDisplayManager?
        {
            ToggleUI();
        }
    }

    public void CloseShopUI()
    {
        /*Debug.Log("Closing Shop");
        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
        playerShop.enabled = false;
        foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
        {
            child.gameObject.SetActive(false);
        }*/
        ShopDisplayManager.CloseShopUI();
    }

    public void ToggleUI()
    {
/*        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
        playerShop.enabled = !playerShop.enabled;
        foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
        {
            child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
        }*/
        ShopDisplayManager.ToggleShopUI();
    }


    /// <summary>
    /// This Server-Rpc attempts to move the camera towards the players current location
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    void MoveCameraServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
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
        //MoveServerAuth();
    }

    /// <summary>
    /// Runs all update logic for the client who owns the champion
    /// </summary>
    private void OwnerUpdate()
    {
        if (!IsOwner) { return; }
        //RotatePlayer();
        updatePointsUI();
        //TryApplyAimPosition();
    }

    

    private void updatePointsUI()
    {
        //Debug.Log(points.text); This was pissing me off, so I commented it out. - H
        points.text = PointManager.Instance.GetPoints(NetworkManager.Singleton.LocalClientId).ToString();
    }

    ///// <summary>
    ///// Calls the server to use the primary ability the champion has
    ///// </summary>
    ///// <param name="context"></param>
    //public void UsePrimaryAbility(InputAction.CallbackContext context)
    //{
    //    if (!IsOwner) return;
    //    if (!context.performed) return;
    //    CastAbilityServerRpc(0);
    //}

    ///// <summary>
    ///// Casts the units secondary ability in tab 0 if it exists.
    ///// </summary>
    ///// <param name="context"></param>
    //public void UseSecondaryAbility(InputAction.CallbackContext context)
    //{
    //    if (!IsOwner) return;

    //    if (!context.performed) return;

    //    if (championAbilityManager.AbilityTabs[0].Abilities.Count < 2)
    //    {
    //        Debug.LogWarning("No secondary ability available.");
    //        return;
    //    }

    //    CastAbilityServerRpc(1);
    //}

    ///// <summary>
    ///// Casts the units secondary ability in tab 0 if it exists.
    ///// </summary>
    ///// <param name="context"></param>
    //public void Use3rdAbility(InputAction.CallbackContext context)
    //{
    //    if (!IsOwner) return;

    //    if (!context.performed) return;

    //    if (championAbilityManager.AbilityTabs[0].Abilities.Count < 3)
    //    {
    //        Debug.LogWarning("No 3rd ability available.");
    //        return;
    //    }

    //    CastAbilityServerRpc(2);
    //}

    ///// <summary>
    ///// Casts the units secondary ability in tab 0 if it exists.
    ///// </summary>
    ///// <param name="context"></param>
    //public void Use4thAbility(InputAction.CallbackContext context)
    //{
    //    if (!IsOwner) return;

    //    if (!context.performed) return;

    //    if (championAbilityManager.AbilityTabs[0].Abilities.Count < 4)
    //    {
    //        Debug.LogWarning("No 4th ability available.");
    //        return;
    //    }

    //    CastAbilityServerRpc(3);
    //}

    ///// <summary>
    ///// Casts the ability relevant to the parsed index. By calling the Ability's Activate() function
    ///// </summary>
    ///// <param name="_AbilityIndex"></param>
    //[ServerRpc(RequireOwnership = false)]
    //private void CastAbilityServerRpc(int _AbilityIndex) // TODO: Should really be moved into abilityManager or something
    //{
    //    championAbilityManager.TryCastAbility(_AbilityIndex, 0);
    //}

    ///// <summary>
    ///// This calls all of the Movement based Server-Rpcs
    ///// </summary>
    //void MoveServerAuth()
    //{
    //    if (!IsServer)
    //    {
    //        Debug.LogError("Client attempted to move the player!");
    //        return;
    //    }
    //    ChampionMove(movementVector);
    //    SetAnimationParams(movementVector);
    //}

    ///// <summary>
    ///// This attempts to move the player transform by adding the movementVector to its current transform
    ///// </summary>
    ///// <param name="_movementVector"></param>
    ///// <param name="serverRpcParams"></param>
    //private void ChampionMove(Vector3 _movementVector)
    //{
    //    if (!IsServer)
    //    {
    //        Debug.LogError("Client attempted to move the player!");
    //        return;
    //    }

    //    _movementVector = Quaternion.Euler(movementRotationOffset) * _movementVector;

    //    Vector3 move = Vector3.right * _movementVector.x + Vector3.forward * _movementVector.z;

    //    Vector3 targetVelocity = move * statManager.CurrentStats[StatType.MoveSpeed];

    //    float lerpSpeed = (_movementVector.magnitude > 0.1f) ? acceleration : deceleration; // Lerp speed changes based on if we're accelerating or decelerating

    //    // lerp towards targetVelocity
    //    velocity = Vector3.MoveTowards(velocity, targetVelocity, lerpSpeed * Time.deltaTime);

    //    if (characterController.isGrounded && velocity.y < 0)
    //    {
    //        velocity.y = -1f; // TODO: Magic number
    //    }
    //    else
    //    {
    //        velocity.y += gravity * Time.deltaTime;
    //    }

    //    // Movement application
    //    characterController.Move(velocity * Time.deltaTime);
    //    //Debug.Log(velocity);
    //}

    ///// <summary>
    ///// The unity input system uses this function to capture the input for the player movement
    ///// </summary>
    ///// <param name="context"></param>
    //public void CheckMove(InputAction.CallbackContext context)
    //{
    //    Vector3 newMovementVector = new Vector3();
    //    newMovementVector.x = context.ReadValue<Vector2>().x;
    //    newMovementVector.y = 0;
    //    newMovementVector.z = context.ReadValue<Vector2>().y;

    //    SetMoveInputServerRpc(newMovementVector);
    //}

    //[ServerRpc]
    //private void SetMoveInputServerRpc(Vector3 _newMovementVector)
    //{
    //    movementVector = _newMovementVector;
    //}

    /// <summary>
    /// Updates the animator controller with the movement vector relative to the rotation
    /// </summary>
    /// <param name="_movementInput"></param>
    /// 
    private void SetAnimationParams(Vector3 _movementInput)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to update the animations!");
            return;
        }

        _movementInput = Quaternion.Euler(movementRotationOffset) * _movementInput;

        if (_movementInput.sqrMagnitude < 0.001f) // Smooth lerp to zero when idle
        {
            nAnimator.SetFloat("MoveX", Mathf.Lerp(nAnimator.GetFloat("MoveX"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("MoveY", Mathf.Lerp(nAnimator.GetFloat("MoveY"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("SpeedX", Mathf.Lerp(nAnimator.GetFloat("SpeedX"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("SpeedY", Mathf.Lerp(nAnimator.GetFloat("SpeedY"), 0f, smoothSpeed * Time.deltaTime));
            return;
        }

        // Normalize input to find local direction (relative)
        Vector3 inputDirection = _movementInput.normalized;
        float relativeX = Vector3.Dot(inputDirection, transform.right); // .Dot() Exists!! 
        float relativeZ = Vector3.Dot(inputDirection, transform.forward);


        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Smoothly update animation parameters
        nAnimator.SetFloat("MoveX", Mathf.Lerp(nAnimator.GetFloat("MoveX"), relativeX, smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("MoveY", Mathf.Lerp(nAnimator.GetFloat("MoveY"), relativeZ, smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("SpeedX", Mathf.Lerp(nAnimator.GetFloat("SpeedX"), Mathf.Abs(localVelocity.x), smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("SpeedY", Mathf.Lerp(nAnimator.GetFloat("SpeedY"), Mathf.Abs(localVelocity.z), smoothSpeed * Time.deltaTime));
    }


    ///// <summary>
    ///// This function rotates the player to face the current position of the mouse
    ///// </summary>
    //public void RotatePlayer()
    //{
    //    if (health.IsDying)
    //    {
    //        return;
    //    }

    //    Vector3 direction = (worldPosition - transform.position).normalized;

    //    if (direction == Vector3.zero)
    //    {
    //        return;
    //    }

    //    RotateCharacterYRpc(direction);
    //}


    ///// <summary>
    ///// This Server-Rpc runs TransformLookAt for the inputted floats as a vector3
    ///// </summary>
    ///// <param name="_x"></param>
    ///// <param name="_y"></param>
    ///// <param name="_z"></param>
    //[Rpc(SendTo.Server)]
    //private void RotateCharacterYRpc(Vector3 _direction)
    //{
    //    if (health.IsDying) // TODO: This being ran in the first place when dying is a bit iffy
    //    {
    //        return;
    //    }

    //    float RotationSpeed = statManager.CurrentStats[StatType.RotationSpeed];

    //    Quaternion targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
    //    Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    //    Vector3 newEuler = newRotation.eulerAngles;

    //    //Vector3 currentEuler = transform.rotation.eulerAngles;
    //    transform.rotation = Quaternion.Euler(0, newEuler.y, 0);
    //}

    //public void SetTarget(Collider castTarget) // TODO: this will be updated in I believe 0.7?? - H
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void ClearTarget()
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void Lunge(float distance, Vector3 direction, float duration)
    //{
    //    StartCoroutine(LungeRoutine(distance, direction.normalized, duration));
    //}

    //private IEnumerator LungeRoutine(float distance, Vector3 direction, float duration)
    //{
    //    float elapsed = 0f;
    //    float speed = distance / duration;

    //    while (elapsed < duration)
    //    {
    //        float step = speed * Time.deltaTime;
    //        characterController.Move(direction * step);
    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }
    //}

    //public void ReviveObject()
    //{
    //    ToggleControlsRpc(true);
    //}

    //public void DestroyObject()
    //{
    //    ToggleControlsRpc(false);
    //    SpawnSoul();
    //}

    //private void SpawnSoul()
    //{
    //    GameObject soul = Instantiate(soulPrefab, transform.position + soulSpawnOffset, Quaternion.identity);
    //    soul.GetComponent<NetworkObject>().Spawn();
    //    soul.GetComponent<ReviveSoul>().Init(gameObject);
        
    //}

    [Rpc(SendTo.Owner)]
    private void ToggleControlsRpc(bool _value)
    {
        if (!IsOwner)
        {
            Debug.LogError($"Client attempted to toggle controls on a non-owner client in gameobject: {gameObject.name}!");
            return;
        }

        playerInput.enabled = _value;
    }

    public void Lunge(float distance, Vector3 direction, float lungeDuration)
    {
        throw new System.NotImplementedException();
    }

    public void SetTarget(Collider castTarget)
    {
        throw new System.NotImplementedException();
    }

    public void ClearTarget()
    {
        throw new System.NotImplementedException();
    }

    public void ReviveObject()
    {
        throw new System.NotImplementedException();
    }

    public void DestroyObject()
    {
        throw new System.NotImplementedException();
    }
}
