using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class AnimatedChampion : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 4f; //movement speed multiplier
    RelayManager manager; //relay manager instance
    Rigidbody rb; //rigidbody attached to the player

    Vector3 movementVector; //the movement vector to be added to the transform
    CameraSpawner cameraSpawner; //camera spawner instance
    NetworkObject networkObject; // current networkObject attached to the player

    Vector3 worldPosition; // the position of the mouse relative to the world origin
    public Vector3 WorldPosition => worldPosition;

    GameObject playerCamera; // the camera that the player will be seeing the game through

    //Vector2 mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

    private Animator animator;
    private AbilityManager abilityManager;

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

        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
            //cameraSpawner.SpawnedCamera.transform.SetParent(transform);
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
        if (!IsOwner) { return; }
        MoveServerAuth();
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
        //AnimatedMove(movementVector);
        MoveServerRpc(movementVector);
        UpdateAnimationParamsServerRpc(movementVector);
        //MoveCameraServerRpc();
    }

    /// <summary>
    /// This Server-Rpc attempts to move the player transform by adding the movementVector to its current transform
    /// </summary>
    /// <param name="movementVector"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 movementVector, ServerRpcParams serverRpcParams = default)
    {
        transform.position += movementVector * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// The unity input system uses this function to capture the input for the player movement
    /// </summary>
    /// <param name="context"></param>
    public void CheckMove(InputAction.CallbackContext context)
    {
        movementVector.x = context.ReadValue<Vector2>().x;
        movementVector.z = context.ReadValue<Vector2>().y; 
    }

    /// <summary>
    /// Updates the animator controller with the movement vector relative to the rotation
    /// </summary>
    /// <param name="_movementInput"></param>
    /// 
    [ServerRpc(RequireOwnership = false)]
    private void UpdateAnimationParamsServerRpc(Vector3 _movementInput)
    {
        if (_movementInput.sqrMagnitude < 0.001f) // Smoothing even when we're standing still
        {
            animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), 0f, 5f * Time.deltaTime));
            animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), 0f, 5f * Time.deltaTime));
            return;
        }

        Vector3 input = _movementInput.normalized;
        float relativeX = Vector3.Dot(input, transform.right); // .Dot() Exists!! 
        float relativeZ = Vector3.Dot(input, transform.forward);

        animator.SetFloat("MoveX", Mathf.Lerp(animator.GetFloat("MoveX"), relativeX, 5.0f * Time.deltaTime));
        animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), relativeZ, 5.0f * Time.deltaTime));

        animator.SetFloat("SpeedX", moveSpeed);
        animator.SetFloat("SpeedY", moveSpeed);
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
