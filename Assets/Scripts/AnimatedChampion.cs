using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class AnimatedChampion : NetworkBehaviour
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

    private GameObject playerCamera; // the camera that the player will be seeing the game through

    private Animator animator;
    private AbilityManager abilityManager;
    private CharacterController characterController;
    private PlayerInput playerInput;


    private Vector3 velocity; // used for gravity shit

    [SerializeField] private Ability primaryAbility;
    void Start()
    {
        manager = RelayManager.Instance;
        rb = GetComponent<Rigidbody>();
        //manager.CreatePlayerServerRpc();

        if (!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError("Skissue");
        }

        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
        }

        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator is required for AnimatedChampion");
        }
        if (!TryGetComponent<AbilityManager>(out abilityManager))
        {
            Debug.LogError("AbilityManager is required for AnimatedChampion");
        }
        if (!TryGetComponent<CharacterController>(out characterController))
        {
            Debug.LogError("CharacterController is required for AnimatedChampion");
        }

        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
            //cameraSpawner.SpawnedCamera.transform.SetParent(transform);
            if (!TryGetComponent<PlayerInput>(out playerInput))
            {
                Debug.LogError("CharacterController is required for AnimatedChampion");
            }
            playerInput.enabled = true;

        }

        Cursor.lockState = CursorLockMode.Confined;

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
        MoveServerAuth();
    }

    /// <summary>
    /// Runs all update logic for the client who owns the champion
    /// </summary>
    private void OwnerUpdate()
    {
        if (!IsOwner) { return; }
        RotatePlayer();
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

    public void UseSecondaryAbility(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (!context.performed) return;

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
    void MoveServerAuth()
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to move the player!");
            return;
        }
        ChampionMove(movementVector);
        SetAnimationParams(movementVector);
    }

    /// <summary>
    /// This attempts to move the player transform by adding the movementVector to its current transform
    /// </summary>
    /// <param name="movementVector"></param>
    /// <param name="serverRpcParams"></param>
    private void ChampionMove(Vector3 movementVector)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to move the player!");
            return;
        }

        Vector3 move = Vector3.right * movementVector.x + Vector3.forward * movementVector.z;

        Vector3 targetVelocity = move * moveSpeed;

        float lerpSpeed = (movementVector.magnitude > 0.1f) ? acceleration : deceleration; // Lerp speed changes based on if we're accelerating or decelerating

        // lerp towards targetVelocity
        velocity = Vector3.MoveTowards(velocity, targetVelocity, lerpSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -1f; // Keeps grounded nicely (adjustable)
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; // Gravity accumulates
        }

        // Movement application
        characterController.Move(velocity * Time.deltaTime);
        //Debug.Log(velocity);
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

        SetMoveInputServerRpc(newMovementVector);
    }

    [ServerRpc]
    private void SetMoveInputServerRpc(Vector3 _newMovementVector)
    {
        movementVector = _newMovementVector;
    }

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

        if (_movementInput.sqrMagnitude < 0.001f) // Smooth lerp to zero when idle
        {
            animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), 0f, smoothSpeed * Time.deltaTime));
            animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), 0f, smoothSpeed * Time.deltaTime));
            animator.SetFloat("SpeedX", Mathf.Lerp(animator.GetFloat("SpeedX"), 0f, smoothSpeed * Time.deltaTime));
            animator.SetFloat("SpeedY", Mathf.Lerp(animator.GetFloat("SpeedY"), 0f, smoothSpeed * Time.deltaTime));
            return;
        }

        // Normalize input to find local direction (relative)
        Vector3 inputDirection = _movementInput.normalized;
        float relativeX = Vector3.Dot(inputDirection, transform.right); // .Dot() Exists!! 
        float relativeZ = Vector3.Dot(inputDirection, transform.forward);


        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Smoothly update animation parameters
        animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), relativeX, smoothSpeed * Time.deltaTime));
        animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), relativeZ, smoothSpeed * Time.deltaTime));
        animator.SetFloat("SpeedX", Mathf.Lerp(animator.GetFloat("SpeedX"), Mathf.Abs(localVelocity.x), smoothSpeed * Time.deltaTime));
        animator.SetFloat("SpeedY", Mathf.Lerp(animator.GetFloat("SpeedY"), Mathf.Abs(localVelocity.z), smoothSpeed * Time.deltaTime));
    }


    /// <summary>
    /// This function rotates the player to face the current position of the mouse
    /// </summary>
    public void RotatePlayer()
    {
        RaycastHit hit;
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(castPoint, out hit))
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
        this.transform.LookAt(new Vector3(x, this.transform.position.y, z));
    }
}
