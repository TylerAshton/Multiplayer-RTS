using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public struct InputPayload : INetworkSerializable
{
    public int tick;
    public Vector3 inputVector;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref inputVector);
    }
}

/// <summary>
/// Representation of the current transformation state within a specific tick
/// </summary>
public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 postition;
    public Quaternion rotation;
    public Vector3 velocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref postition);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
    }
}


[RequireComponent(typeof(Animator))]
public class NETChamp : NetworkBehaviour, IAbilityUser, IFaction
{
    [SerializeField] private float moveSpeed = 4f; //movement speed multiplier
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

    private GameObject playerCamera; // the camera that the player will be seeing the game through

    private NetCodeAnimationManager nAnimator;
    private AbilityManager abilityManager;
    private CharacterController characterController;
    private PlayerInput playerInput;

    private Vector3 velocity; // used for gravity shit

    [SerializeField] private Ability primaryAbility;

    private UIManager uiManager;
    private PlayerManager playerManager;

    [SerializeField] private TextMeshProUGUI points;

    public bool inShop = false;

    [SerializeField] InputReader input;


    // Netcode General Variables
    [Header("Netcode")]
    NetworkTimer timer;
    const float k_serverTickRate = 60f;
    const int k_bufferSize = 1024;
    [SerializeField] float reconciliationCooldownTime = 1f;
    [SerializeField] CountdownTimer reconciliationCooldown;

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
    private float reconciliationThreshold = 10f;

    [Header("Netcode Debug")]
    [SerializeField] GameObject serverCube;
    [SerializeField] GameObject clientCube;


    private void Awake()
    {
        timer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();

        reconciliationCooldown = new CountdownTimer(reconciliationCooldownTime);
    }


    void Start()
    {
        manager = RelayManager.Instance;
        uiManager = UIManager.Instance;
        playerManager = PlayerManager.Instance;
        rb = GetComponent<Rigidbody>();

        if (!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError("Skissue");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
        }

        if (!TryGetComponent<NetCodeAnimationManager>(out nAnimator))
        {
            Debug.LogError("NetCodeAnimationManager is required for AnimatedChampion");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("AbilityManager is required for AnimatedChampion");
        }
        if (!TryGetComponent<AbilityPositionManager>(out abilityPositionManager))
        {
            Debug.LogError("AbilityPositionManager is required for AnimatedChampion");
        }
        if (!TryGetComponent<CharacterController>(out characterController))
        {
            Debug.LogError("CharacterController is required for AnimatedChampion");
        }
        if (!TryGetComponent<EffectManager>(out effectManager))
        {
            Debug.LogError("EffectManager is required for AnimatedChampion");
        }

        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
            if (!TryGetComponent<PlayerInput>(out playerInput))
            {
                Debug.LogError("CharacterController is required for AnimatedChampion");
            }
            input.Enable();

        }

        Cursor.lockState = CursorLockMode.Confined;

    }

    public void AttemptToggleUI()
    {
        if (inShop && networkObject.IsOwner)
        {
            ToggleUI();
        }
    }

    public void CloseShopUI()
    {
        Debug.Log("Closing Shop");
        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
        playerShop.enabled = false;
        foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ToggleUI()
    {
        Shop playerShop = gameObject.GetComponentInChildren<Shop>(true);
        playerShop.enabled = !playerShop.enabled;
        foreach (RectTransform child in playerShop.GetComponentInChildren<RectTransform>(true))
        {
            child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
        }
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
        timer.Update(Time.deltaTime);
        reconciliationCooldown.Tick(Time.deltaTime);
        //ServerUpdate();
        //OwnerUpdate();
    }

    private void FixedUpdate()
    {
        //RotatePlayer();
        //updatePointsUI();
        while (timer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }
    }

    void HandleServerTick()
    {
        if (!IsServer) { return; }
        var bufferIndex = -1;
        while (serverInputQueue.Count > 0)
        {
            InputPayload inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            //StatePayload statePayload = SimulateMovement(inputPayload);
            StatePayload statePayload = ProcessMovement(inputPayload);

            serverCube.transform.position = new (statePayload.postition.x, statePayload.postition.y + 5, statePayload.postition.z);
            
            serverStateBuffer.Add(statePayload, bufferIndex);
            
        }

        if (bufferIndex == -1) { return; }
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        //SetAnimationParams(serverStateBuffer.Get(bufferIndex).postition);

    }

    //StatePayload SimulateMovement(InputPayload inputPayload)
    //{
    //    Physics.simulationMode = SimulationMode.Script;

    //    Move(inputPayload.inputVector);
        
    //    Physics.Simulate(Time.fixedDeltaTime);
    //    Physics.simulationMode = SimulationMode.FixedUpdate;

    //    return new StatePayload()
    //    {
    //        tick = inputPayload.tick,
    //        postition = transform.position,
    //        rotation = transform.rotation,
    //        velocity = characterController.velocity
    //    };
    //}


    [Rpc(SendTo.NotServer)]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner) { return; }
        lastServerState = statePayload;
    }

    void HandleClientTick()
    {
        if (!IsClient || !IsOwner) {  return; }

        var currentTick = timer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            inputVector = input.Move //<------------ Look Here!
        };

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);
        StatePayload statePayload = ProcessMovement(inputPayload);

        clientCube.transform.position = new(statePayload.postition.x, statePayload.postition.y + 5, statePayload.postition.z);

        clientStateBuffer.Add(statePayload, bufferIndex);

        

        // HandleServerReconciliation();
    }

    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        //bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(obj: default) || !lastProcessedState.Equals(lastServerState);
        bool isLastStateUndefinedOrDifferent;
        if (lastProcessedState.Equals(default) || !lastProcessedState.Equals(lastServerState))
        {
            isLastStateUndefinedOrDifferent = true;
        }
        else
        {
            isLastStateUndefinedOrDifferent = false;
        }
            return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationCooldown.IsRunning;
    }

    void HandleServerReconciliation()
    {
        if(!ShouldReconcile()) { return; }

        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) { return; } // Not enough info to reconcile

        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;  // Due to host having 0 latency between rpc we can directly grab the last state if its the host.
        positionError = Vector3.Distance(rewindState.postition, clientStateBuffer.Get(bufferIndex).postition);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
        }

        lastProcessedState = lastServerState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.postition;
        transform.rotation = rewindState.rotation;
        characterController.velocity.Set(rewindState.velocity.x, rewindState.velocity.y, rewindState.velocity.z);

        if (!rewindState.Equals(lastServerState)) { return; }

        clientStateBuffer.Add(rewindState, rewindState.tick);

        int tickToReplay = lastServerState.tick;

        while (tickToReplay < timer.CurrentTick)
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
            postition = transform.position,
            rotation = transform.rotation,
            velocity = characterController.velocity
        };
    }

    private void updatePointsUI()
    {
        if (!IsOwner) {  return; }
        Debug.Log(points.text);
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

        if (abilityManager.AbilityTabs[0].Abilities.Count < 2)
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
        abilityManager.TryCastAbility(_AbilityIndex);
    }

    /// <summary>
    /// This calls all of the Movement based Server-Rpcs
    /// </summary>
    void Move(Vector2 inputVector)
    {
        //if (!IsOwner) {  return; }
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
    /// <param name="movementVector"></param>
    /// <param name="serverRpcParams"></param>
    private void ChampionMove(Vector3 movementVector)
    {
        Vector3 move = Vector3.right * movementVector.x + Vector3.forward * movementVector.z;

        Vector3 targetVelocity = move * moveSpeed;

        float lerpSpeed = (movementVector.magnitude > 0.1f) ? acceleration : deceleration; // Lerp speed changes based on if we're accelerating or decelerating

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
    //    movementVector = newMovementVector;
    //    //SetMoveInputServerRpc(newMovementVector);
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
        Debug.Log($"Vel{velocity}");

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
        if (!IsOwner) {  return; }
        RaycastHit hit;
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        LayerMask environmentMask = LayerMask.GetMask("Environment");
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, environmentMask))
        {
            worldPosition = hit.point;
        };
        this.transform.LookAt(new Vector3(worldPosition.x, this.transform.position.y, worldPosition.z));
    }


    ///// <summary>
    ///// This Server-Rpc runs TransformLookAt for the inputted floats as a vector3
    ///// </summary>
    ///// <param name="x"></param>
    ///// <param name="y"></param>
    ///// <param name="z"></param>
    //[ServerRpc(RequireOwnership = false)]
    //private void RotationServerRpc(float x, float y, float z)
    //{
        
    //}
}
