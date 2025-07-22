using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IShopUser
{
    ChampionAbilityManager ChampionAbilityManager { get; }
    Health ChampionHealth { get; }
    int Points { get; } 
    ShopPurchaseManager ShopPurchaseManager { get; }

    ulong PlayerID { get; }
}

public struct InputPayload : INetworkSerializable
{
    public int tick;
    public DateTime timestamp;
    public ulong networkObjectId;
    public Vector3 inputVector;
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref timestamp);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref position);
    }
}

/// <summary>
/// Representation of the current transformation state within a specific tick
/// </summary>
public struct StatePayload : INetworkSerializable
{
    public int tick;
    public ulong networkObjectId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
    }
}

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

    private NetCodeAnimationManager nAnimator;
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

    [SerializeField] InputReader input;

    ClientNetworkTransform clientNetworkTransform;

    // Netcode General Variables
    [Header("Netcode")]
    [SerializeField] CountdownTimer reconciliationTimer;
    [SerializeField] float reconciliationCooldownTime = 1f;
    [SerializeField] private float reconciliationThreshold = 10f; // Used to be 50f
    [SerializeField] float extrapolationLimit = 0.5f; // 1f = 1000 Milliseconds therefore 0.5f = 500 milliseconds
    [SerializeField] float extrapolationMultiplier = 1.2f;
    NetworkTimer networkTimer;
    const float k_serverTickRate = 60f;
    const int k_bufferSize = 1024;

    // Netcode Client Specific Variables
    [Header("Netcode Client")]
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;
    StatePayload lastProcessedState;

    // Netcode Server Specific Variables
    [Header("Netcode Server")]
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;


    [Header("Netcode Debug")]
    [SerializeField] GameObject serverCube;
    [SerializeField] GameObject clientCube;

    StatePayload extrapolationState;
    CountdownTimer extrapolationTimer;

    private void Awake()
    {
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();

        networkTimer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();

        reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
        extrapolationTimer = new CountdownTimer(0);  // Replacing "extrapolationLimit" with "0" would disable extrapolation

        reconciliationTimer.OnTimerStart += () =>
        {
            extrapolationTimer.Stop();
        };

        extrapolationTimer.OnTimerStart += () =>
        {
            reconciliationTimer.Stop();
            SwitchAuthorityMode(AuthorityMode.Server);
        };

        extrapolationTimer.OnTimerStop += () =>
        {
            extrapolationState = default;
            SwitchAuthorityMode(AuthorityMode.Client);
        };
    }

    void SwitchAuthorityMode(AuthorityMode mode)
    {
        clientNetworkTransform.Auth = mode; // The server should take control when the client is severely lagging and stop syncing position
        bool shouldSync = mode == AuthorityMode.Client;
        clientNetworkTransform.SyncPositionX = shouldSync;
        clientNetworkTransform.SyncPositionY = shouldSync;
        clientNetworkTransform.SyncPositionZ = shouldSync;
    }

    void Start()
    {
        manager = RelayManager.Instance;
        uiManager = UIManager.Instance;
        playerManager = PlayerManager.Instance;
        rb = GetComponent<Rigidbody>();

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
            //playerInput.enabled = true;
            input.Enable();
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
        //mouseScreenPos = context.ReadValue<Vector2>();
        mouseScreenPos = input.Point;
        worldPosition = new Vector3(0, 0, 0);

        Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);
        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, environmentMask))
        {
            worldPosition = hit.point;
        }
    }

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
        networkTimer.Update(Time.deltaTime);
        reconciliationTimer.Tick(Time.deltaTime);
        extrapolationTimer.Tick(Time.deltaTime);
        ServerUpdate();
        OwnerUpdate();
    }

    private void FixedUpdate()
    {
        while (networkTimer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }
        Extrapolate();
    }

    void HandleClientTick()
    {
        if (!IsClient || !IsOwner) { return; }

        var currentTick = networkTimer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            timestamp = DateTime.Now,
            networkObjectId = NetworkObjectId,
            inputVector = input.Move,
            position = transform.position
        };

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);
        StatePayload statePayload = ProcessMovement(inputPayload);

        //clientCube.transform.position = new(statePayload.position.x, statePayload.position.y + 5, statePayload.position.z); //Debug Code

        clientStateBuffer.Add(statePayload, bufferIndex);

        HandleServerReconciliation();
    }

    void HandleServerTick()
    {
        if (!IsServer) { return; }
        var bufferIndex = -1;
        InputPayload inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            //StatePayload statePayload = SimulateMovement(inputPayload);
            if (IsHost)
            {
                StatePayload statePay = new StatePayload()
                {
                    tick = inputPayload.tick,
                    networkObjectId = NetworkObjectId,
                    position = transform.position,
                    rotation = transform.rotation,
                    velocity = characterController.velocity
                };
                //StatePayload statePay = ProcessMovement(inputPayload);
                serverStateBuffer.Add(statePay, bufferIndex);
                SendToClientRpc(statePay);
                continue;
            }

            StatePayload statePayload = ProcessMovement(inputPayload);
            //serverCube.transform.position = new(statePayload.position.x, statePayload.position.y + 5, statePayload.position.z); // DEBUG CUBE
            serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) { return; }
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
        //SetAnimationParams(serverStateBuffer.Get(bufferIndex).postition);
    }

    void Extrapolate()
    {
        if (IsServer && extrapolationTimer.IsRunning)
        {
            Debug.Log("Extrapolating");
            transform.position += new Vector3(extrapolationState.position.x, 0f, extrapolationState.position.z);
        }
    }

    void HandleExtrapolation(StatePayload latest, float latency)
    {
        if (ShouldExtrapolate(latency)) // if unacceptable ammount of latency
        {
            if (extrapolationState.position != default)
            {
                latest = extrapolationState;
            }

            var posAdjustment = latest.velocity * (1 + latency * extrapolationMultiplier);
            extrapolationState.position = posAdjustment;
            //extrapolationState.position = latest.position;
            extrapolationState.rotation = latest.rotation;
            extrapolationState.velocity = latest.velocity;
            extrapolationTimer.Start();
        }
        else
        {
            extrapolationTimer.Stop();
        }
    }

    private bool ShouldExtrapolate(float latency)
    {
        return (latency > extrapolationLimit) && (latency > Time.fixedDeltaTime);
    }

    StatePayload SimulateMovement(InputPayload inputPayload)
    {
        Physics.simulationMode = SimulationMode.Script;

        Move(inputPayload.inputVector);

        Physics.Simulate(Time.fixedDeltaTime);
        Physics.simulationMode = SimulationMode.FixedUpdate;

        return new StatePayload()
        {
            tick = inputPayload.tick,
            networkObjectId = NetworkObjectId,
            position = transform.position,
            rotation = transform.rotation,
            velocity = characterController.velocity
        };
    }

    static float CalculateLatencyInMillis(InputPayload inputPayload)
    {
        return (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f; // Returns seconds so divide by 1000 to get milliseconds
    }

    [Rpc(SendTo.NotServer)]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner) { return; }
        lastServerState = statePayload;
    }


    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        //bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(obj: default) ||� !lastProcessedState.Equals(lastServerState);
        bool isLastStateUndefinedOrDifferent;
        if (lastProcessedState.Equals(default) || !lastProcessedState.Equals(lastServerState))
        {
            isLastStateUndefinedOrDifferent = true;
        }
        else
        {
            isLastStateUndefinedOrDifferent = false;
        }
        return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
    }

    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) { return; }

        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) { return; } // Not enough info to reconcile

        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;  // Due to host having 0 latency between rpc we can directly grab the last state if its the host.
        StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
        positionError = Vector3.Distance(rewindState.position, clientState.position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
            reconciliationTimer.Start();
        }

        lastProcessedState = lastServerState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;
        characterController.velocity.Set(rewindState.velocity.x, rewindState.velocity.y, rewindState.velocity.z);

        if (!rewindState.Equals(lastServerState)) { return; }

        clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

        int tickToReplay = lastServerState.tick;

        while (tickToReplay < networkTimer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

    [Rpc(SendTo.Server)]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        Move(input.inputVector);

        return new StatePayload()
        {
            tick = input.tick,
            networkObjectId = input.networkObjectId,
            position = transform.position,
            rotation = transform.rotation,
            velocity = characterController.velocity
        };
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
        RotatePlayer();
        updatePointsUI();
        TryApplyAimPosition();
    }

    

    private void updatePointsUI()
    {
        //Debug.Log(points.text); This was pissing me off, so I commented it out. - H
        points.text = PointManager.Instance.GetPoints(NetworkManager.Singleton.LocalClientId).ToString();
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
    /// Casts the ability relevant to the parsed index. By calling the Ability's Activate() function
    /// </summary>
    /// <param name="_AbilityIndex"></param>
    [ServerRpc(RequireOwnership = false)]
    private void CastAbilityServerRpc(int _AbilityIndex)
    {
        championAbilityManager.TryCastAbility(_AbilityIndex);
    }

    /// <summary>
    /// This calls all of the Movement based Server-Rpcs
    /// </summary>
    void Move(Vector2 inputVector)
    {
        Vector3 newMovementVector = new Vector3();
        newMovementVector.x = inputVector.x;
        newMovementVector.y = 0;
        newMovementVector.z = inputVector.y;
        movementVector = newMovementVector;
        ChampionMove(movementVector);
        SetAnimationParams(movementVector);
        RotatePlayer();
    }

    /// <summary>
    /// This attempts to move the player transform by adding the movementVector to its current transform
    /// </summary>
    /// <param name="_movementVector"></param>
    /// <param name="serverRpcParams"></param>
    private void ChampionMove(Vector3 _movementVector)
    {
        _movementVector = Quaternion.Euler(movementRotationOffset) * _movementVector;

        Vector3 move = Vector3.right * _movementVector.x + Vector3.forward * _movementVector.z;

        Vector3 targetVelocity = move * statManager.CurrentStats[StatType.MoveSpeed];

        float lerpSpeed = (_movementVector.magnitude > 0.1f) ? acceleration : deceleration; // Lerp speed changes based on if we're accelerating or decelerating

        // lerp towards targetVelocity
        velocity = Vector3.MoveTowards(velocity, targetVelocity, lerpSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -1f; // TODO: Magic number
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Movement application
        characterController.Move(velocity * Time.deltaTime);
        //Debug.Log(velocity);
    }

    /// <summary>
    /// The unity input system uses this function to capture the input for the player movement
    /// </summary>
    /// <param name="context"></param>
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


    /// <summary>
    /// This function rotates the player to face the current position of the mouse
    /// </summary>
    public void RotatePlayer()
    {
        if (health.IsDying || !IsOwner)
        {
            return;
        }

        RaycastHit hit;
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        LayerMask environmentMask = LayerMask.GetMask("Environment");
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, environmentMask)) // TODO: wtf is this doing here, we have a mouse pos var and world pos var
        {
            worldPosition = hit.point;
        };

        RotationServerRpc(worldPosition.x, worldPosition.y, worldPosition.z);
    }


    /// <summary>
    /// This Server-Rpc runs TransformLookAt for the inputted floats as a vector3
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    [ServerRpc(RequireOwnership = false)]
    private void RotationServerRpc(float x, float y, float z)
    {
        if (health.IsDying) // TODO: This being ran in the first place when dying is a bit iffy
        {
            return;
        }
        this.transform.LookAt(new Vector3(x, this.transform.position.y, z));
    }

    public void SetTarget(Collider castTarget) // TODO: this will be updated in I believe 0.7?? - H
    {
        throw new System.NotImplementedException();
    }

    public void ClearTarget()
    {
        throw new System.NotImplementedException();
    }

    public void Lunge(float distance, Vector3 direction, float duration)
    {
        StartCoroutine(LungeRoutine(distance, direction.normalized, duration));
    }

    private IEnumerator LungeRoutine(float distance, Vector3 direction, float duration)
    {
        float elapsed = 0f;
        float speed = distance / duration;

        while (elapsed < duration)
        {
            float step = speed * Time.deltaTime;
            characterController.Move(direction * step);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void ReviveObject()
    {
        nAnimator.SetTrigger("Revive");
        ToggleControlsRpc(true);
    }

    public void DestroyObject()
    {
        nAnimator.SetTrigger("Death");
        ToggleControlsRpc(false);
        SpawnSoul();
    }

    private void SpawnSoul()
    {
        GameObject soul = Instantiate(soulPrefab, transform.position + soulSpawnOffset, Quaternion.identity);
        soul.GetComponent<NetworkObject>().Spawn();
        soul.GetComponent<ReviveSoul>().Init(gameObject);
        
    }

    [Rpc(SendTo.Owner)]
    private void ToggleControlsRpc(bool _value)
    {
        if (!IsOwner)
        {
            Debug.LogError($"Client attempted to toggle controls on a non-owner client in gameobject: {gameObject.name}!");
            return;
        }

        //playerInput.enabled = _value;
        input.ManualToggle(_value);
    }
}
