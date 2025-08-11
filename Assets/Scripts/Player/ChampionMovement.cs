using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ChampionMovement : NetworkBehaviour
{
    [Header("Movement Values")]
    [HideInInspector]public Vector3 movementVector; // SERVER ONLY the movement vector to be added to the transform
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float gravity = -9.81f;
    [HideInInspector] public Vector3 worldPosition => champControls.WorldPosition;
    private Vector3 velocity; // used for gravity shit
    [HideInInspector] public Vector3 Velocity => velocity;

    [Header("Rotation")]
    [SerializeField] private Vector3 movementRotationOffset = Vector3.zero;

    [Header("Dependencies")]
    private ChampionControls champControls;

    [Header("Managers")]
    public ChampionManager champManager;
    [HideInInspector] public StatManager statManager;

    [HideInInspector] public CharacterController characterController;
    private Health health => champManager.ChampionHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<ChampionControls>(out champControls))
        {
            Debug.LogError($"{nameof(ChampionControls)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
        }
        if (!TryGetComponent<ChampionManager>(out champManager))
        {
            Debug.LogError($"{nameof(ChampionManager)} is required for {GetType().Name} on gameobject {gameObject.name}!");
            return;
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
    /// This Server-Rpc attempts to move the camera towards the players current location
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    void MoveCameraServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
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
        //SetAnimationParams(movementVector);   ----------- This will be in the animation script
    }

    /// <summary>
    /// This attempts to move the player transform by adding the movementVector to its current transform
    /// </summary>
    /// <param name="_movementVector"></param>
    /// <param name="serverRpcParams"></param>
    private void ChampionMove(Vector3 _movementVector)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to move the player!");
            return;
        }

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
    /// This function rotates the player to face the current position of the mouse
    /// </summary>
    public void RotatePlayer()
    {
        if (health.IsDying)
        {
            return;
        }

        Vector3 direction = (worldPosition - transform.position).normalized;

        if (direction == Vector3.zero)
        {
            return;
        }

        RotateCharacterYRpc(direction);
    }

    /// <summary>
    /// This Server-Rpc runs TransformLookAt for the inputted floats as a vector3
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    [Rpc(SendTo.Server)]
    private void RotateCharacterYRpc(Vector3 _direction)
    {
        if (health.IsDying) // TODO: This being ran in the first place when dying is a bit iffy
        {
            return;
        }

        float RotationSpeed = statManager.CurrentStats[StatType.RotationSpeed];

        Quaternion targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        Vector3 newEuler = newRotation.eulerAngles;

        //Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, newEuler.y, 0);
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
}
