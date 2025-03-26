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

    Transform cameraPosition;
    Vector3 diff;

    //Vector2 mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

    void Start()
    {
        diff = Vector3.back;
        worldPosition = Vector3.back;

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
            cameraPosition = cameraSpawner.SpawnedCamera.transform;
            CameraParentServerRpc();
            //cameraSpawner.SpawnedCamera.transform.SetParent(transform);
        }

        Cursor.lockState = CursorLockMode.Confined;

    }

    [ServerRpc(RequireOwnership = false)]
    void CameraParentServerRpc()
    {
        transform.SetParent(cameraPosition);
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
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveServerRpc(Vector3 movementVector)
    {
        rb.linearVelocity = movementVector * moveSpeed;
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
            Debug.Log(worldPosition);

            diff = worldPosition - transform.position;

        };



        GetRotationServerRpc();
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void GetRotationServerRpc()
    {
        //float yrot = Mathf.Tan(diff.x/diff.z);
        //Debug.Log(yrot * (180 / Mathf.PI));
        //this.transform.rotation = new (transform.rotation.x, (yrot * (180 / Mathf.PI)), transform.rotation.z, 1);
        //this.transform.rotation = Quaternion.LookRotation(diff, Vector3.up);
        
        this.transform.rotation = Quaternion.LookRotation(diff.normalized);

        Debug.DrawRay(transform.position, diff, Color.red);
        Debug.Log(diff);
    }
}
