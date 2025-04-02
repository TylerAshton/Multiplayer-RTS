using System.Globalization;
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

    GameObject playerCamera; // the camera that the player will be seeing the game through

    //Vector2 mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

    private Animator animator;

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
        /*if (!IsOwner) { return; }*/ // TODO: Uncomment
        MoveServerAuth();
        RotatePlayer();
    }

    /// <summary>
    /// This calls all of the Movement based Server-Rpcs
    /// </summary>
    void MoveServerAuth()
    {
        AnimatedMove(movementVector);
        UpdateAnimationParams(movementVector);
        //MoveServerRpc(movementVector); // TODO: Rework
        //MoveCameraServerRpc();
    }

    private void AnimatedMove(Vector3 _movementVector)
    {
        transform.position += _movementVector * Time.deltaTime * moveSpeed;
    }

    /// <summary>
    /// This Server-Rpc attempts to move the player transform by adding the movementVector to its current transform
    /// </summary>
    /// <param name="movementVector"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 movementVector, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        player.transform.position += movementVector * Time.fixedDeltaTime;
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
    /// Updates the animator controller with the movement vector
    /// </summary>
    /// <param name="_movementInput"></param>
    private void UpdateAnimationParams(Vector3 _movementInput)
    {
        float currentX = animator.GetFloat("MoveX");
        //Debug.Log(currentX);
        animator.SetFloat("MoveX", Mathf.Lerp(currentX, _movementInput.x, 5.0f * Time.deltaTime));
        animator.SetFloat("MoveY", Mathf.Lerp(animator.GetFloat("MoveY"), _movementInput.z, 5.0f * Time.deltaTime));
        //animator.SetFloat("MoveY", _movementInput.z);
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

        //GetRotationServerRpc(diff.x, diff.y, diff.z);
        GetRotationServerRpc(worldPosition.x, worldPosition.y, worldPosition.z);
    }


    /// <summary>
    /// This Server-Rpc attempts to 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    [ServerRpc(RequireOwnership = false)]
    private void GetRotationServerRpc(float x, float y, float z)
    {
        //Vector3 remakeDiff = new Vector3(x, y, z);
        //float yrot = Mathf.Acos((Vector3.Dot(remakeDiff, -transform.position))/(Vector3.Magnitude(remakeDiff)*Vector3.Magnitude(-transform.position)));
        //this.transform.rotation = Quaternion.LookRotation(new Vector3(x, this.transform.rotation.y, z), Vector3.up);

        //Debug.Log((yrot * (180 / Mathf.PI)));

        this.transform.LookAt(new Vector3(x, this.transform.position.y, z));

        //this.transform.rotation = new Quaternion(this.transform.rotation.x, (yrot * (180 / Mathf.PI)), this.transform.rotation.z, this.transform.rotation.w);
    }
}
