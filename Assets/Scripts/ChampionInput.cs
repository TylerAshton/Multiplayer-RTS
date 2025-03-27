using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;

public class ChampionInput : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    RelayManager manager;
    Rigidbody rb;

    float moveHorizontal, moveVertical;
    Vector3 movementVector;
    CameraSpawner cameraSpawner;
    NetworkObject networkObject;

    Vector3 worldPosition;

    GameObject playerCamera;
    Vector3 diff;

    //Vector2 mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

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

        if (networkObject.IsOwner)
        {
            cameraSpawner.Init();
            playerCamera = cameraSpawner.SpawnedCamera.transform.gameObject;
            //cameraSpawner.SpawnedCamera.transform.SetParent(transform);
        }

        Cursor.lockState = CursorLockMode.Confined;

    }

    [ServerRpc(RequireOwnership = false)]
    void MoveCameraServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        CameraMovement 
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        MoveServerAuth();
        RotatePlayer();
    }

    void MoveServerAuth()
    {
        MoveServerRpc(movementVector);
        MoveCameraServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 movementVector, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        player.transform.position += movementVector * Time.fixedDeltaTime;
    }

    public void CheckMove(InputAction.CallbackContext context)
    {
        movementVector.x = context.ReadValue<Vector2>().x;
        movementVector.z = context.ReadValue<Vector2>().y;
    }

    public void RotatePlayer()
    {
        RaycastHit hit;
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(castPoint, out hit))
        {
            worldPosition = hit.point;

            diff = worldPosition - transform.position;
        };

        //GetRotationServerRpc(diff.x, diff.y, diff.z);
        GetRotationServerRpc(worldPosition.x, worldPosition.y, worldPosition.z);
    }

    
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
